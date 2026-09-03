using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly List<Equipment> _equipment = new();

    public InMemoryEquipmentRepository()
    {
        _equipment.Add(new Equipment { Id = 1, Name = "Multimeter", IsAvailable = true });
        _equipment.Add(new Equipment { Id = 2, Name = "Oscilloscope", IsAvailable = false });
    }

    public Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_equipment.FirstOrDefault(e => e.Id == id));
    }

    public Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        var index = _equipment.FindIndex(e => e.Id == equipment.Id);
        if (index != -1) _equipment[index] = equipment;
        return Task.CompletedTask;
    }
}