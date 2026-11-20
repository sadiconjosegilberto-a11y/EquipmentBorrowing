using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowingQueryService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowingQueryService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public Task<IReadOnlyList<Student>> GetAllStudentsAsync(CancellationToken cancellationToken = default) =>
        _studentRepository.GetAllAsync(cancellationToken);

    public Task<IReadOnlyList<Equipment>> GetEquipmentCatalogAsync(CancellationToken cancellationToken = default) =>
        _equipmentRepository.GetAllAsync(cancellationToken);

    public Task<IReadOnlyList<Equipment>> GetAvailableEquipmentAsync(CancellationToken cancellationToken = default) =>
        _equipmentRepository.GetAvailableAsync(cancellationToken);

    public Task<IReadOnlyList<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default) =>
        _borrowingRepository.GetAllActiveAsync(cancellationToken);

    public async Task<IReadOnlyList<Borrowing>> GetStudentHistoryAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken)
            ?? throw new InvalidOperationException("Selected student no longer exists.");

        return await _borrowingRepository.GetByStudentIdAsync(student.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetEquipmentHistoryAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken)
            ?? throw new InvalidOperationException("Selected equipment item no longer exists.");

        return await _borrowingRepository.GetByEquipmentIdAsync(equipment.Id, cancellationToken);
    }

    public Task<IReadOnlyList<Borrowing>> SearchBorrowingsAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        BorrowingSortField sortBy,
        bool ascending,
        CancellationToken cancellationToken = default) =>
        _borrowingRepository.SearchAsync(searchText, statusFilter, sortBy, ascending, cancellationToken);
}