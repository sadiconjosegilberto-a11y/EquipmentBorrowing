using EquipmentBorrowing.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task<IEnumerable<Borrowing>> GetAllAsync(CancellationToken cancellationToken = default);
}