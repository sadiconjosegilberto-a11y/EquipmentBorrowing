using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings = new();

    public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        borrowing.Id = _borrowings.Count + 1;
        _borrowings.Add(borrowing);
        return Task.CompletedTask;
    }

    public Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_borrowings.FirstOrDefault(b => b.Id == id));
    }

    public Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        var index = _borrowings.FindIndex(b => b.Id == borrowing.Id);
        if (index != -1) _borrowings[index] = borrowing;
        return Task.CompletedTask;
    }
    public Task<IEnumerable<Borrowing>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Borrowing>>(_borrowings);
    }
}