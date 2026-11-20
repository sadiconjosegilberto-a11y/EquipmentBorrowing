using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentResult
{
    public bool Success { get; }
    public bool IsUnknownOutcome { get; }
    public string? FailureReason { get; }
    public Borrowing? Borrowing { get; }
    public IReadOnlyList<Borrowing> Borrowings { get; }

    private BorrowEquipmentResult(
        bool success,
        bool isUnknownOutcome,
        string? failureReason,
        IReadOnlyList<Borrowing> borrowings)
    {
        Success = success;
        IsUnknownOutcome = isUnknownOutcome;
        FailureReason = failureReason;
        Borrowings = borrowings;
        Borrowing = borrowings.Count > 0 ? borrowings[0] : null;
    }

    public static BorrowEquipmentResult Fail(string reason, bool isUnknownOutcome = false) =>
        new(false, isUnknownOutcome, reason, Array.Empty<Borrowing>());

    public static BorrowEquipmentResult Ok(IReadOnlyList<Borrowing> borrowings) =>
        new(true, false, null, borrowings);
}

public class BorrowEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowEquipmentService(IBorrowingRepository borrowingRepository)
    {
        _borrowingRepository = borrowingRepository;
    }

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
        : this(borrowingRepository)
    {
    }

    public Task<BorrowEquipmentResult> ExecuteAsync(
        int studentId,
        int equipmentId,
        DateTime expectedReturnDate,
        CancellationToken cancellationToken = default)
    {
        if (studentId <= 0)
            return Task.FromResult(BorrowEquipmentResult.Fail("A valid student must be selected."));

        if (equipmentId <= 0)
            return Task.FromResult(BorrowEquipmentResult.Fail("A valid equipment item must be selected."));

        return ExecuteCoreAsync(studentId, new[] { equipmentId }, expectedReturnDate, null, cancellationToken);
    }

    public Task<BorrowEquipmentResult> CheckoutMultipleAsync(
        int studentId,
        IReadOnlyList<int> equipmentIds,
        DateTime expectedReturnDate,
        Func<Task>? beforeCommitHook = null,
        CancellationToken cancellationToken = default)
    {
        if (studentId <= 0)
            return Task.FromResult(BorrowEquipmentResult.Fail("A valid student must be selected."));

        if (equipmentIds is null || equipmentIds.Count < 2)
            return Task.FromResult(BorrowEquipmentResult.Fail("Multi-item checkout requires at least two distinct equipment items."));

        if (equipmentIds.Any(id => id <= 0))
            return Task.FromResult(BorrowEquipmentResult.Fail("One or more selected equipment items are invalid."));

        if (equipmentIds.Distinct().Count() != equipmentIds.Count)
            return Task.FromResult(BorrowEquipmentResult.Fail("Duplicate equipment items are not allowed in a multi-item checkout."));

        return ExecuteCoreAsync(studentId, equipmentIds, expectedReturnDate, beforeCommitHook, cancellationToken);
    }

    private async Task<BorrowEquipmentResult> ExecuteCoreAsync(
        int studentId,
        IReadOnlyList<int> equipmentIds,
        DateTime expectedReturnDate,
        Func<Task>? beforeCommitHook,
        CancellationToken cancellationToken)
    {
        var checkoutDate = DateTime.Today;
        if (expectedReturnDate == default || expectedReturnDate.Date <= checkoutDate)
            return BorrowEquipmentResult.Fail("Expected return date must be later than today's checkout date.");

        try
        {
            var created = await _borrowingRepository.CheckoutItemsAsync(
                studentId,
                equipmentIds,
                checkoutDate,
                expectedReturnDate.Date,
                beforeCommitHook,
                cancellationToken);

            return BorrowEquipmentResult.Ok(created);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BorrowEquipmentResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            var (unknown, msg) = PersistenceErrorHelper.Describe(ex);
            return BorrowEquipmentResult.Fail(msg, unknown);
        }
    }
}