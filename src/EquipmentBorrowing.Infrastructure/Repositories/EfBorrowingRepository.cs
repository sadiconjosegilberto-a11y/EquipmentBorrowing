using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfBorrowingRepository : IBorrowingRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfBorrowingRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await _context.Borrowings.AddAsync(borrowing, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active, cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .Where(b => b.Status == BorrowingStatus.Active)
            .OrderByDescending(b => b.DateBorrowed)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _context.Borrowings.Update(borrowing);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .Where(b => b.EquipmentId == equipmentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        BorrowingSortField sortBy,
        bool ascending,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(b => b.Status == statusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLower();
            query = query.Where(b =>
                (b.Student != null && (b.Student.Name.ToLower().Contains(term)
                                    || b.Student.StudentNumber.ToLower().Contains(term))) ||
                (b.Equipment != null && b.Equipment.Name.ToLower().Contains(term)));
        }

        query = (sortBy, ascending) switch
        {
            (BorrowingSortField.CheckoutDate, true) => query.OrderBy(b => b.DateBorrowed).ThenBy(b => b.Id),
            (BorrowingSortField.CheckoutDate, false) => query.OrderByDescending(b => b.DateBorrowed).ThenBy(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, true) => query.OrderBy(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, false) => query.OrderByDescending(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            _ => query.OrderByDescending(b => b.DateBorrowed).ThenBy(b => b.Id)
        };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> CheckoutItemsAsync(
        int studentId,
        IReadOnlyList<int> equipmentIds,
        DateTime checkoutDate,
        DateTime expectedReturnDate,
        Func<Task>? beforeCommitHook = null,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            ?? throw new InvalidOperationException("Selected student does not exist.");

        if (!student.IsAllowedToBorrow)
            throw new InvalidOperationException("Selected student is restricted from borrowing.");

        var activeCount = await _context.Borrowings
            .CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active, cancellationToken);

        if (activeCount + equipmentIds.Count > 3)
            throw new InvalidOperationException($"Borrowing limit exceeded. Student currently has {activeCount} active item(s); maximum is 3.");

        var distinctIds = equipmentIds.Distinct().ToList();
        if (distinctIds.Count != equipmentIds.Count)
            throw new InvalidOperationException("Duplicate equipment items are not permitted in a single checkout.");

        var equipmentList = await _context.Equipment
            .Where(e => distinctIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        if (equipmentList.Count != distinctIds.Count)
            throw new InvalidOperationException("One or more selected equipment items no longer exist.");

        var activeEquipmentIds = await _context.Borrowings
            .Where(b => distinctIds.Contains(b.EquipmentId) && b.Status == BorrowingStatus.Active)
            .Select(b => b.EquipmentId)
            .ToListAsync(cancellationToken);

        if (equipmentList.Any(e => !e.IsAvailable) || activeEquipmentIds.Count > 0)
            throw new InvalidOperationException("One or more selected equipment items are currently unavailable.");

        var createdBorrowings = new List<Borrowing>(distinctIds.Count);
        foreach (var equipment in equipmentList)
        {
            var borrowing = new Borrowing(studentId, equipment.Id, checkoutDate, expectedReturnDate);
            equipment.MarkBorrowed();
            await _context.Borrowings.AddAsync(borrowing, cancellationToken);
            createdBorrowings.Add(borrowing);
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (beforeCommitHook is not null)
        {
            await beforeCommitHook();
        }

        await transaction.CommitAsync(cancellationToken);
        return createdBorrowings;
    }

    public async Task ReturnBorrowingAsync(
        int borrowingId,
        DateTime returnDate,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var borrowing = await _context.Borrowings
            .FirstOrDefaultAsync(b => b.Id == borrowingId, cancellationToken)
            ?? throw new InvalidOperationException("Selected borrowing record does not exist.");

        if (borrowing.Status != BorrowingStatus.Active)
            throw new InvalidOperationException("Selected borrowing has already been returned.");

        var equipment = await _context.Equipment
            .FirstOrDefaultAsync(e => e.Id == borrowing.EquipmentId, cancellationToken)
            ?? throw new InvalidOperationException("Associated equipment record does not exist.");

        borrowing.MarkReturned(returnDate);
        equipment.MarkReturned();

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}