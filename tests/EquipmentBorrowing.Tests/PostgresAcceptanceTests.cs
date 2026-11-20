using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EquipmentBorrowing.Tests;

public class PostgresAcceptanceTests : IAsyncLifetime
{
    private readonly string _testConnectionString;

    public PostgresAcceptanceTests()
    {
        var baseConn = Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_CONNECTION")
            ?? throw new InvalidOperationException("Set EQUIPMENT_BORROWING_CONNECTION before running tests.");

        var builder = new NpgsqlConnectionStringBuilder(baseConn)
        {
            Database = "equipment_borrowing_test"
        };
        _testConnectionString = builder.ConnectionString;
    }

    public async Task InitializeAsync()
    {
        await using var ctx = CreateFreshContext();
        await ctx.Database.EnsureDeletedAsync();
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var ctx = CreateFreshContext();
        await ctx.Database.EnsureDeletedAsync();
    }

    private EquipmentBorrowingDbContext CreateFreshContext(string? overrideConn = null)
    {
        var options = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
            .UseNpgsql(overrideConn ?? _testConnectionString)
            .Options;
        return new EquipmentBorrowingDbContext(options);
    }

    private ServiceProvider BuildFreshServices(string? overrideConn = null)
    {
        var services = new ServiceCollection();
        services.AddDbContext<EquipmentBorrowingDbContext>(
            opt => opt.UseNpgsql(overrideConn ?? _testConnectionString),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        services.AddTransient<IStudentRepository, EfStudentRepository>();
        services.AddTransient<IEquipmentRepository, EfEquipmentRepository>();
        services.AddTransient<IBorrowingRepository, EfBorrowingRepository>();

        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();
        services.AddTransient<StudentManagementService>();
        services.AddTransient<EquipmentManagementService>();
        services.AddTransient<BorrowingQueryService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task A01_A02_CatalogAndAvailableEquipment_MatchesDatabaseAndHandlesEmptyAndFailure()
    {
        await using var sp = BuildFreshServices();
        var query = sp.GetRequiredService<BorrowingQueryService>();
        var borrow = sp.GetRequiredService<BorrowEquipmentService>();

        var allInitial = await query.GetEquipmentCatalogAsync();
        var availInitial = await query.GetAvailableEquipmentAsync();
        Assert.NotEmpty(allInitial);
        Assert.Equal(allInitial.Count, availInitial.Count);

        var student = (await query.GetAllStudentsAsync()).First(s => s.IsAllowedToBorrow);
        var itemToBorrow = availInitial.First();
        var res = await borrow.ExecuteAsync(student.Id, itemToBorrow.Id, DateTime.Today.AddDays(5));
        Assert.True(res.Success);

        // Fresh readback
        await using var verifySp = BuildFreshServices();
        var verifyQuery = verifySp.GetRequiredService<BorrowingQueryService>();
        var allAfter = await verifyQuery.GetEquipmentCatalogAsync();
        var availAfter = await verifyQuery.GetAvailableEquipmentAsync();

        Assert.Equal(allInitial.Count, allAfter.Count);
        Assert.Equal(availInitial.Count - 1, availAfter.Count);
        Assert.DoesNotContain(availAfter, e => e.Id == itemToBorrow.Id);

        // Failed connection check
        await using var badSp = BuildFreshServices("Host=localhost;Port=59999;Database=nonexistent;Username=bad;Password=bad;Timeout=1");
        var badQuery = badSp.GetRequiredService<BorrowingQueryService>();
        await Assert.ThrowsAnyAsync<Exception>(() => badQuery.GetEquipmentCatalogAsync());
    }

    [Fact]
    public async Task A03_A04_A05_SingleBorrow_ActiveList_And_Return_PersistAcrossFreshReads()
    {
        await using var sp = BuildFreshServices();
        var query = sp.GetRequiredService<BorrowingQueryService>();
        var borrow = sp.GetRequiredService<BorrowEquipmentService>();
        var ret = sp.GetRequiredService<ReturnEquipmentService>();
        var studentMgmt = sp.GetRequiredService<StudentManagementService>();

        var eligible = (await query.GetAllStudentsAsync()).First(s => s.IsAllowedToBorrow);
        var restricted = (await studentMgmt.RegisterAsync("STU-RESTRICTED", "Restricted User", null)).Student!;
        await studentMgmt.ChangeEligibilityAsync(restricted.Id, false);

        var items = (await query.GetAvailableEquipmentAsync()).Take(4).ToList();

        // Reject invalid date, missing student, missing item, ineligible student
        Assert.False((await borrow.ExecuteAsync(eligible.Id, items[0].Id, DateTime.Today)).Success);
        Assert.False((await borrow.ExecuteAsync(999999, items[0].Id, DateTime.Today.AddDays(3))).Success);
        Assert.False((await borrow.ExecuteAsync(eligible.Id, 999999, DateTime.Today.AddDays(3))).Success);
        Assert.False((await borrow.ExecuteAsync(restricted.Id, items[0].Id, DateTime.Today.AddDays(3))).Success);

        // Borrow 3 items (up to limit) and reject 4th (over-limit) and unavailable item
        var b1 = await borrow.ExecuteAsync(eligible.Id, items[0].Id, DateTime.Today.AddDays(3));
        var b2 = await borrow.ExecuteAsync(eligible.Id, items[1].Id, DateTime.Today.AddDays(4));
        var b3 = await borrow.ExecuteAsync(eligible.Id, items[2].Id, DateTime.Today.AddDays(5));
        Assert.True(b1.Success && b2.Success && b3.Success);

        Assert.False((await borrow.ExecuteAsync(eligible.Id, items[0].Id, DateTime.Today.AddDays(3))).Success); // unavailable
        Assert.False((await borrow.ExecuteAsync(eligible.Id, items[3].Id, DateTime.Today.AddDays(3))).Success); // over limit (BR-09)

        // Fresh readback (simulating restart) for A03/A04
        await using var restartSp1 = BuildFreshServices();
        var restartQuery1 = restartSp1.GetRequiredService<BorrowingQueryService>();
        var active = await restartQuery1.GetActiveBorrowingsAsync();
        Assert.Contains(active, b => b.Id == b1.Borrowing!.Id && b.Student != null && b.Equipment != null);

        // A05 Return + repeat return rejection + missing borrowing rejection
        var returnOk = await restartSp1.GetRequiredService<ReturnEquipmentService>().ExecuteAsync(b1.Borrowing!.Id);
        Assert.True(returnOk.Success);
        Assert.False((await restartSp1.GetRequiredService<ReturnEquipmentService>().ExecuteAsync(b1.Borrowing!.Id)).Success);
        Assert.False((await restartSp1.GetRequiredService<ReturnEquipmentService>().ExecuteAsync(999999)).Success);

        // Fresh readback after return
        await using var restartSp2 = BuildFreshServices();
        var activeAfterReturn = await restartSp2.GetRequiredService<BorrowingQueryService>().GetActiveBorrowingsAsync();
        Assert.DoesNotContain(activeAfterReturn, b => b.Id == b1.Borrowing!.Id);

        var returnedItem = (await restartSp2.GetRequiredService<BorrowingQueryService>().GetEquipmentCatalogAsync())
            .First(e => e.Id == items[0].Id);
        Assert.True(returnedItem.IsAvailable);
    }

    [Fact]
    public async Task A06_MultiItemCheckout_EnforcesRulesAndRollsBackAtomicallyOnFailure()
    {
        await using var sp = BuildFreshServices();
        var query = sp.GetRequiredService<BorrowingQueryService>();
        var borrow = sp.GetRequiredService<BorrowEquipmentService>();

        var student = (await query.GetAllStudentsAsync()).First(s => s.IsAllowedToBorrow);
        var items = (await query.GetAvailableEquipmentAsync()).Take(4).ToList();

        // Reject < 2 items, duplicates, invalid date, over-limit
        Assert.False((await borrow.CheckoutMultipleAsync(student.Id, new[] { items[0].Id }, DateTime.Today.AddDays(3))).Success);
        Assert.False((await borrow.CheckoutMultipleAsync(student.Id, new[] { items[0].Id, items[0].Id }, DateTime.Today.AddDays(3))).Success);
        Assert.False((await borrow.CheckoutMultipleAsync(student.Id, new[] { items[0].Id, items[1].Id }, DateTime.Today)).Success);
        Assert.False((await borrow.CheckoutMultipleAsync(student.Id, items.Select(i => i.Id).ToList(), DateTime.Today.AddDays(3))).Success);

        // Controlled in-transaction failure after write is attempted
        var failedHookResult = await borrow.CheckoutMultipleAsync(
            student.Id,
            new[] { items[0].Id, items[1].Id },
            DateTime.Today.AddDays(3),
            beforeCommitHook: () => throw new InvalidOperationException("Simulated mid-transaction failure."));

        Assert.False(failedHookResult.Success);

        // Fresh readback: zero borrowings created and both items remain available
        await using var verifySp1 = BuildFreshServices();
        var verifyQuery1 = verifySp1.GetRequiredService<BorrowingQueryService>();
        Assert.Empty(await verifyQuery1.GetActiveBorrowingsAsync());
        var catalog1 = await verifyQuery1.GetEquipmentCatalogAsync();
        Assert.True(catalog1.First(e => e.Id == items[0].Id).IsAvailable);
        Assert.True(catalog1.First(e => e.Id == items[1].Id).IsAvailable);

        // Valid atomic checkout of 3 items (at exact limit)
        var validBatch = await borrow.CheckoutMultipleAsync(
            student.Id,
            new[] { items[0].Id, items[1].Id, items[2].Id },
            DateTime.Today.AddDays(4));
        Assert.True(validBatch.Success);
        Assert.Equal(3, validBatch.Borrowings.Count);

        await using var verifySp2 = BuildFreshServices();
        var activeAfterBatch = await verifySp2.GetRequiredService<BorrowingQueryService>().GetActiveBorrowingsAsync();
        Assert.Equal(3, activeAfterBatch.Count);
    }

    [Fact]
    public async Task A07_A08_A09_HistoriesAndSearchFilterSort_ReturnAccurateRecords()
    {
        await using var sp = BuildFreshServices();
        var studentMgmt = sp.GetRequiredService<StudentManagementService>();
        var equipMgmt = sp.GetRequiredService<EquipmentManagementService>();
        var borrow = sp.GetRequiredService<BorrowEquipmentService>();
        var ret = sp.GetRequiredService<ReturnEquipmentService>();
        var query = sp.GetRequiredService<BorrowingQueryService>();

        var s1 = (await studentMgmt.RegisterAsync("2026-0001", "Alice Santos", "alice@cmu.edu.ph")).Student!;
        var s2 = (await studentMgmt.RegisterAsync("2026-0002", "Bob Reyes", null)).Student!;
        var sEmpty = (await studentMgmt.RegisterAsync("2026-0003", "Charlie Cruz", null)).Student!;

        var e1 = (await equipMgmt.RegisterAsync("LAB-901", "Fluke Scope", "Handheld")).Equipment!;
        var e2 = (await equipMgmt.RegisterAsync("LAB-902", "Rigol Generator", null)).Equipment!;
        var eEmpty = (await equipMgmt.RegisterAsync("LAB-903", "Unused Sensor", null)).Equipment!;

        var b1 = (await borrow.ExecuteAsync(s1.Id, e1.Id, DateTime.Today.AddDays(2))).Borrowing!;
        var b2 = (await borrow.ExecuteAsync(s2.Id, e2.Id, DateTime.Today.AddDays(6))).Borrowing!;
        await ret.ExecuteAsync(b1.Id);

        // A07 Student History isolation
        var s1History = await query.GetStudentHistoryAsync(s1.Id);
        Assert.Single(s1History);
        Assert.Equal(BorrowingStatus.Returned, s1History[0].Status);
        Assert.Empty(await query.GetStudentHistoryAsync(sEmpty.Id));
        await Assert.ThrowsAsync<InvalidOperationException>(() => query.GetStudentHistoryAsync(999999));

        // A08 Equipment History isolation
        var e2History = await query.GetEquipmentHistoryAsync(e2.Id);
        Assert.Single(e2History);
        Assert.Equal(BorrowingStatus.Active, e2History[0].Status);
        Assert.Empty(await query.GetEquipmentHistoryAsync(eEmpty.Id));
        await Assert.ThrowsAsync<InvalidOperationException>(() => query.GetEquipmentHistoryAsync(999999));

        // A09 Search, Filter, Sort
        var byNumber = await query.SearchBorrowingsAsync("  2026-0001 ", null, BorrowingSortField.CheckoutDate, true);
        Assert.Single(byNumber);

        var activeOnly = await query.SearchBorrowingsAsync(null, BorrowingStatus.Active, BorrowingSortField.ExpectedReturnDate, false);
        Assert.Single(activeOnly);
        Assert.Equal(b2.Id, activeOnly[0].Id);

        var noMatch = await query.SearchBorrowingsAsync("DOES-NOT-EXIST", null, BorrowingSortField.CheckoutDate, true);
        Assert.Empty(noMatch);
    }

    [Fact]
    public async Task A12_A13_A14_StudentManagementAndEligibility_PreserveHistoryAndEnforceUniqueness()
    {
        await using var sp = BuildFreshServices();
        var studentMgmt = sp.GetRequiredService<StudentManagementService>();
        var borrow = sp.GetRequiredService<BorrowEquipmentService>();
        var ret = sp.GetRequiredService<ReturnEquipmentService>();
        var query = sp.GetRequiredService<BorrowingQueryService>();

        // A12 Register + duplicate/blank rejection
        var created = await studentMgmt.RegisterAsync("  st-100  ", "  Maria Clara  ", "09171234567");
        Assert.True(created.Success);
        Assert.False((await studentMgmt.RegisterAsync("ST-100", "Duplicate", null)).Success);
        Assert.False((await studentMgmt.RegisterAsync("   ", "Blank Number", null)).Success);
        Assert.False((await studentMgmt.RegisterAsync("ST-101", "   ", null)).Success);

        var item = (await query.GetAvailableEquipmentAsync()).First();
        var activeBorrowing = (await borrow.ExecuteAsync(created.Student!.Id, item.Id, DateTime.Today.AddDays(5))).Borrowing!;

        // A13 Update + clear contact + preserve history
        var updated = await studentMgmt.UpdateAsync(created.Student.Id, "ST-100-REV", "Maria Clara Updated", "   ");
        Assert.True(updated.Success);
        Assert.Null(updated.Student!.ContactInfo);

        // A14 Restrict -> block new issue -> return existing -> permit -> issue allowed
        var restrict1 = await studentMgmt.ChangeEligibilityAsync(created.Student.Id, false);
        var restrictRepeat = await studentMgmt.ChangeEligibilityAsync(created.Student.Id, false);
        Assert.True(restrict1.Success && restrictRepeat.Success);

        var secondItem = (await query.GetAvailableEquipmentAsync()).First();
        Assert.False((await borrow.ExecuteAsync(created.Student.Id, secondItem.Id, DateTime.Today.AddDays(5))).Success);

        // Existing borrowing can still be returned while restricted
        Assert.True((await ret.ExecuteAsync(activeBorrowing.Id)).Success);

        // Permit again -> new borrowing succeeds
        Assert.True((await studentMgmt.ChangeEligibilityAsync(created.Student.Id, true)).Success);
        Assert.True((await borrow.ExecuteAsync(created.Student.Id, secondItem.Id, DateTime.Today.AddDays(5))).Success);

        // Restart readback
        await using var verifySp = BuildFreshServices();
        var persistedStudent = (await verifySp.GetRequiredService<BorrowingQueryService>().GetAllStudentsAsync())
            .First(s => s.Id == created.Student.Id);
        Assert.Equal("ST-100-REV", persistedStudent.StudentNumber);
        Assert.Null(persistedStudent.ContactInfo);
        Assert.True(persistedStudent.IsAllowedToBorrow);
    }

    [Fact]
    public async Task A15_A16_EquipmentManagement_EnforcesUniquenessAndPreservesAvailabilityAndHistory()
    {
        await using var sp = BuildFreshServices();
        var equipMgmt = sp.GetRequiredService<EquipmentManagementService>();
        var borrow = sp.GetRequiredService<BorrowEquipmentService>();
        var query = sp.GetRequiredService<BorrowingQueryService>();

        var reg = await equipMgmt.RegisterAsync("  eq-777 ", "  Oscilloscope Pro  ", "4-channel");
        Assert.True(reg.Success);
        Assert.True(reg.Equipment!.IsAvailable);
        Assert.False((await equipMgmt.RegisterAsync("EQ-777", "Duplicate", null)).Success);
        Assert.False((await equipMgmt.RegisterAsync("", "No Code", null)).Success);

        var student = (await query.GetAllStudentsAsync()).First(s => s.IsAllowedToBorrow);
        await borrow.ExecuteAsync(student.Id, reg.Equipment.Id, DateTime.Today.AddDays(4));

        // Update descriptive fields while borrowed; availability must remain false and history intact
        var upd = await equipMgmt.UpdateAsync(reg.Equipment.Id, "EQ-777-B", "Oscilloscope Pro Rev B", "  ");
        Assert.True(upd.Success);

        await using var verifySp = BuildFreshServices();
        var verifyQuery = verifySp.GetRequiredService<BorrowingQueryService>();
        var persistedEquip = (await verifyQuery.GetEquipmentCatalogAsync()).First(e => e.Id == reg.Equipment.Id);
        Assert.Equal("EQ-777-B", persistedEquip.Identifier);
        Assert.Null(persistedEquip.Description);
        Assert.False(persistedEquip.IsAvailable);
        Assert.Single(await verifyQuery.GetEquipmentHistoryAsync(reg.Equipment.Id));
    }

    [Fact]
    public async Task A17_DatabaseConstraints_RejectInvalidFkDuplicatesAndSecondActiveBorrowingDirectly()
    {
        await using var ctx = CreateFreshContext();

        // 1. Invalid Foreign Key
        ctx.Borrowings.Add(new Borrowing(999999, 999999, DateTime.Today, DateTime.Today.AddDays(3)));
        await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        ctx.ChangeTracker.Clear();

        // 2. Duplicate normalized student number at DB level
        ctx.Students.Add(new Student("dup-01", "Student One"));
        await ctx.SaveChangesAsync();
        ctx.Students.Add(new Student("  DUP-01 ", "Student Two"));
        await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        ctx.ChangeTracker.Clear();

        // 3. Second active borrowing for the same equipment at DB level (BR-10 / BR-16)
        var studentId = await ctx.Students.Select(s => s.Id).FirstAsync();
        var equipId = await ctx.Equipment.Select(e => e.Id).FirstAsync();
        ctx.Borrowings.Add(new Borrowing(studentId, equipId, DateTime.Today, DateTime.Today.AddDays(3)));
        await ctx.SaveChangesAsync();

        ctx.Borrowings.Add(new Borrowing(studentId, equipId, DateTime.Today, DateTime.Today.AddDays(4)));
        await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
    }
}