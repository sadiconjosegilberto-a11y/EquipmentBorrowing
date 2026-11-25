using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class PostgresDatabaseInitializer : IDatabaseInitializer
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _contextFactory;

    public PostgresDatabaseInitializer(IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
    }
}