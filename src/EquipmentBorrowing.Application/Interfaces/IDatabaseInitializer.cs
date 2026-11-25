using System.Threading;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}