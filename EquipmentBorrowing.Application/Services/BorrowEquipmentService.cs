using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
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

    public async Task<bool> BorrowAsync(int studentId, int equipmentId, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student == null || !student.IsAllowedToBorrow || student.ActiveBorrowingCount >= student.MaxActiveBorrowings)
            return false;

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment == null || !equipment.IsAvailable)
            return false;

        var borrowing = new Borrowing
        {
            StudentId = studentId,
            EquipmentId = equipmentId,
            BorrowedDate = DateTime.UtcNow,
            ExpectedReturnDate = DateTime.UtcNow.AddDays(7)
        };

        student.ActiveBorrowingCount++;
        equipment.IsAvailable = false;

        await _borrowingRepository.AddAsync(borrowing, cancellationToken);
        await _studentRepository.UpdateAsync(student, cancellationToken);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return true;
    }
}