using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfBorrowingRepository : IBorrowingRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfBorrowingRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await _context.Borrowings.AddAsync(borrowing, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        // Fixed: Compare against BorrowingStatus enum value instead of string
        return await _context.Borrowings
            .Where(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        // Fixed: Compare against BorrowingStatus enum value instead of string
        return await _context.Borrowings
            .AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .Where(b => b.Status == BorrowingStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _context.Borrowings.Update(borrowing);
        await _context.SaveChangesAsync(cancellationToken);
    }
}