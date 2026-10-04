using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfEquipmentRepository : IEquipmentRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfEquipmentRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Uses the Equipment DbSet defined in the DbContext
        return await _context.Equipment.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Matches GetAllAsync signature returning IReadOnlyList[cite: 3]
        return await _context.Equipment.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        // Fulfills the UpdateAsync requirement[cite: 3]
        _context.Equipment.Update(equipment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}