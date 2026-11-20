using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class EquipmentBorrowingDbContextFactory : IDesignTimeDbContextFactory<EquipmentBorrowingDbContext>
{
    public EquipmentBorrowingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_CONNECTION")
            ?? throw new InvalidOperationException(
                "Set the EQUIPMENT_BORROWING_CONNECTION environment variable before running migrations.");

        var optionsBuilder = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new EquipmentBorrowingDbContext(optionsBuilder.Options);
    }
}