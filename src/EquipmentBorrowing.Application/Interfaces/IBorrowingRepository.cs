using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        BorrowingSortField sortBy,
        bool ascending,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> CheckoutItemsAsync(
        int studentId,
        IReadOnlyList<int> equipmentIds,
        DateTime checkoutDate,
        DateTime expectedReturnDate,
        Func<Task>? beforeCommitHook = null,
        CancellationToken cancellationToken = default);
    Task ReturnBorrowingAsync(
        int borrowingId,
        DateTime returnDate,
        CancellationToken cancellationToken = default);
}