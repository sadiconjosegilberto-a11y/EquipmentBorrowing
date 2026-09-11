using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings = new();

    public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _borrowings.Add(borrowing);
        return Task.CompletedTask;
    }

    public Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var count = _borrowings.Count(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active);
        return Task.FromResult(count);
    }

    public Task<IReadOnlyList<Borrowing>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Borrowing> list = _borrowings.Where(b => b.Status == BorrowingStatus.Active).ToList();
        return Task.FromResult(list);
    }

    public Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var borrowing = _borrowings.FirstOrDefault(b => b.Id == id);
        return Task.FromResult(borrowing);
    }

    public Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        // Entity is already in the list by reference; no-op needed for in-memory.
        return Task.CompletedTask;
    }
}
