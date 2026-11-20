using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<int, Student> _students = new();

    public void Seed(Student student) => _students[student.Id] = student;

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _students.TryGetValue(id, out var student);
        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Student> list = _students.Values.ToList();
        return Task.FromResult(list);
    }

    public Task<bool> ExistsByNormalizedNumberAsync(
        string normalizedStudentNumber,
        int? excludeStudentId = null,
        CancellationToken cancellationToken = default)
    {
        var exists = _students.Values.Any(s =>
            s.NormalizedStudentNumber == normalizedStudentNumber &&
            (!excludeStudentId.HasValue || s.Id != excludeStudentId.Value));

        return Task.FromResult(exists);
    }

    public Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        if (student.Id == 0)
        {
            var nextId = _students.Count == 0 ? 1 : _students.Keys.Max() + 1;
            typeof(Student).GetProperty(nameof(Student.Id))?.SetValue(student, nextId);
        }

        _students[student.Id] = student;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _students[student.Id] = student;
        return Task.CompletedTask;
    }
}