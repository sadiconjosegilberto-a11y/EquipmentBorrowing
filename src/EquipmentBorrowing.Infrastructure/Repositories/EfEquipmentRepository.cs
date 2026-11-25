using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfEquipmentRepository : IEquipmentRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _contextFactory;

    public EfEquipmentRepository(IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Equipment
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Equipment
            .AsNoTracking()
            .Where(e => idList.Contains(e.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Equipment
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Equipment
            .AsNoTracking()
            .Where(e => e.IsAvailable)
            .OrderBy(e => e.Name)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNormalizedIdentifierAsync(
        string normalizedIdentifier,
        int? excludeEquipmentId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Equipment
            .AsNoTracking()
            .AnyAsync(e => e.NormalizedIdentifier == normalizedIdentifier
                        && (!excludeEquipmentId.HasValue || e.Id != excludeEquipmentId.Value),
                cancellationToken);
    }

    public async Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Equipment.AddAsync(equipment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Equipment.Update(equipment);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Selected equipment record no longer exists.");
        }
    }
}