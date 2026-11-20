using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentResult
{
    public bool Success { get; }
    public bool IsUnknownOutcome { get; }
    public string? FailureReason { get; }

    private ReturnEquipmentResult(bool success, bool isUnknownOutcome, string? failureReason)
    {
        Success = success;
        IsUnknownOutcome = isUnknownOutcome;
        FailureReason = failureReason;
    }

    public static ReturnEquipmentResult Fail(string reason, bool isUnknownOutcome = false) =>
        new(false, isUnknownOutcome, reason);

    public static ReturnEquipmentResult Ok() => new(true, false, null);
}

public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;

    public ReturnEquipmentService(IBorrowingRepository borrowingRepository)
    {
        _borrowingRepository = borrowingRepository;
    }

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository)
        : this(borrowingRepository)
    {
    }

    public async Task<ReturnEquipmentResult> ExecuteAsync(
        int borrowingId,
        CancellationToken cancellationToken = default)
    {
        if (borrowingId <= 0)
            return ReturnEquipmentResult.Fail("A valid active borrowing must be selected.");

        try
        {
            await _borrowingRepository.ReturnBorrowingAsync(borrowingId, DateTime.Today, cancellationToken);
            return ReturnEquipmentResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return ReturnEquipmentResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            var (unknown, msg) = PersistenceErrorHelper.Describe(ex);
            return ReturnEquipmentResult.Fail(msg, unknown);
        }
    }
}