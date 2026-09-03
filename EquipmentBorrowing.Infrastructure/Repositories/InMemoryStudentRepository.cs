using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students = new();

    public InMemoryStudentRepository()
    {
        _students.Add(new Student { Id = 1, Name = "Alice", IsAllowedToBorrow = true, ActiveBorrowingCount = 0 });
        _students.Add(new Student { Id = 2, Name = "Bob", IsAllowedToBorrow = false, ActiveBorrowingCount = 0 });
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_students.FirstOrDefault(s => s.Id == id));
    }

    public Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        var index = _students.FindIndex(s => s.Id == student.Id);
        if (index != -1) _students[index] = student;
        return Task.CompletedTask;
    }
}