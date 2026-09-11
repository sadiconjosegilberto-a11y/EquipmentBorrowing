using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentResult
{
    public bool Success { get; }
    public string? FailureReason { get; }
    public Borrowing? Borrowing { get; }

    private BorrowEquipmentResult(bool success, string? failureReason, Borrowing? borrowing)
    {
        Success = success;
        FailureReason = failureReason;
        Borrowing = borrowing;
    }

    public static BorrowEquipmentResult Fail(string reason) => new(false, reason, null);
    public static BorrowEquipmentResult Ok(Borrowing borrowing) => new(true, null, borrowing);
}

public class BorrowEquipmentService
{
    private const int MaxActiveBorrowings = 3;

    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<BorrowEquipmentResult> ExecuteAsync(int studentId, int equipmentId, DateTime expectedReturnDate)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student is null)
            return BorrowEquipmentResult.Fail("Student does not exist.");

        if (!student.IsAllowedToBorrow)
            return BorrowEquipmentResult.Fail("Student is not currently allowed to borrow.");

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId);
        if (equipment is null)
            return BorrowEquipmentResult.Fail("Equipment does not exist.");

        if (!equipment.IsAvailable)
            return BorrowEquipmentResult.Fail("Equipment is not currently available.");

        var activeCount = await _borrowingRepository.CountActiveByStudentIdAsync(studentId);
        if (activeCount >= MaxActiveBorrowings)
            return BorrowEquipmentResult.Fail("Student has reached the maximum number of active borrowings.");

        equipment.MarkBorrowed();
        var borrowing = new Borrowing(
            id: new Random().Next(1000, 9999),
            studentId: studentId,
            equipmentId: equipmentId,
            dateBorrowed: DateTime.UtcNow,
            expectedReturnDate: expectedReturnDate);

        await _borrowingRepository.AddAsync(borrowing);
        return BorrowEquipmentResult.Ok(borrowing);
    }
}