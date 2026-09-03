using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using System.Collections.Generic;
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
}