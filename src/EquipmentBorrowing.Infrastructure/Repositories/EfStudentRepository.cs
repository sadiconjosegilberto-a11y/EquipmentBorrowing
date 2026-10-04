using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfStudentRepository : IStudentRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfStudentRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Matches GetByIdAsync signature[cite: 2]
        return await _context.Students.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Returns IReadOnlyList<Student> as required by the interface[cite: 2]
        // AsNoTracking is used here since this is a read-only operation
        return await _context.Students.AsNoTracking().ToListAsync(cancellationToken);
    }
}