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

    public Task<IReadOnlyList<Equipment>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var idSet = ids.ToHashSet();
        IReadOnlyList<Equipment> list = _equipment.Values.Where(e => idSet.Contains(e.Id)).ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Equipment> list = _equipment.Values.ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Equipment> list = _equipment.Values.Where(e => e.IsAvailable).ToList();
        return Task.FromResult(list);
    }

    public Task<bool> ExistsByNormalizedIdentifierAsync(
        string normalizedIdentifier,
        int? excludeEquipmentId = null,
        CancellationToken cancellationToken = default)
    {
        var exists = _equipment.Values.Any(e =>
            e.NormalizedIdentifier == normalizedIdentifier &&
            (!excludeEquipmentId.HasValue || e.Id != excludeEquipmentId.Value));

        return Task.FromResult(exists);
    }

    public Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        if (equipment.Id == 0)
        {
            var nextId = _equipment.Count == 0 ? 1 : _equipment.Keys.Max() + 1;
            typeof(Equipment).GetProperty(nameof(Equipment.Id))?.SetValue(equipment, nextId);
        }

        _equipment[equipment.Id] = equipment;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _equipment[equipment.Id] = equipment;
        return Task.CompletedTask;
    }
}