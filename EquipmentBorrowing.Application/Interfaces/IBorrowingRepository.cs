using EquipmentBorrowing.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
}