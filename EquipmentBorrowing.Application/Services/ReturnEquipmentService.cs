using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public ReturnEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<bool> ReturnAsync(int borrowingId, CancellationToken cancellationToken = default)
    {
        var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId, cancellationToken);
        if (borrowing == null || borrowing.Status == BorrowingStatus.Returned)
            return false;

        var student = await _studentRepository.GetByIdAsync(borrowing.StudentId, cancellationToken);
        var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId, cancellationToken);

        if (student == null || equipment == null)
            return false;

        borrowing.Status = BorrowingStatus.Returned;
        student.ActiveBorrowingCount--;
        equipment.IsAvailable = true;

        await _borrowingRepository.UpdateAsync(borrowing, cancellationToken);
        await _studentRepository.UpdateAsync(student, cancellationToken);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return true;
    }
}