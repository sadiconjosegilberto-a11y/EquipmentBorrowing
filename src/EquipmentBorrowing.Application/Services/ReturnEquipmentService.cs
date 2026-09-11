using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentResult
{
    public bool Success { get; }
    public string? FailureReason { get; }

    private ReturnEquipmentResult(bool success, string? failureReason)
    {
        Success = success;
        FailureReason = failureReason;
    }

    public static ReturnEquipmentResult Fail(string reason) => new(false, reason);
    public static ReturnEquipmentResult Ok() => new(true, null);
}

public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<ReturnEquipmentResult> ExecuteAsync(int borrowingId)
    {
        var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (borrowing is null)
            return ReturnEquipmentResult.Fail("Borrowing record not found.");

        if (borrowing.Status == Domain.BorrowingStatus.Returned)
            return ReturnEquipmentResult.Fail("This borrowing has already been returned.");

        var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId);
        if (equipment is null)
            return ReturnEquipmentResult.Fail("Equipment record not found.");

        borrowing.MarkReturned();
        equipment.MarkReturned();

        await _borrowingRepository.UpdateAsync(borrowing);
        return ReturnEquipmentResult.Ok();
    }
}
