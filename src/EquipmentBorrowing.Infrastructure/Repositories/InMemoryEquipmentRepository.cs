using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly Dictionary<int, Equipment> _equipment = new();

    public void Seed(Equipment equipment) => _equipment[equipment.Id] = equipment;

    public Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _equipment.TryGetValue(id, out var equipment);
        return Task.FromResult(equipment);
    }

    public Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Equipment> list = _equipment.Values.ToList();
        return Task.FromResult(list);
    }
}
