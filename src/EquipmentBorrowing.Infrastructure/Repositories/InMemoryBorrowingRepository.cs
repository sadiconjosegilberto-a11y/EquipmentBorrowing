using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings = new();
    private readonly IStudentRepository? _studentRepository;
    private readonly IEquipmentRepository? _equipmentRepository;

    public InMemoryBorrowingRepository(
        IStudentRepository? studentRepository = null,
        IEquipmentRepository? equipmentRepository = null)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
    }

    public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        if (borrowing.Id == 0)
        {
            var nextId = _borrowings.Count == 0 ? 1 : _borrowings.Max(b => b.Id) + 1;
            typeof(Borrowing).GetProperty(nameof(Borrowing.Id))?.SetValue(borrowing, nextId);
        }

        _borrowings.Add(borrowing);
        return Task.CompletedTask;
    }

    public Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var count = _borrowings.Count(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active);
        return Task.FromResult(count);
    }

    public Task<IReadOnlyList<Borrowing>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Borrowing> list = _borrowings.Where(b => b.Status == BorrowingStatus.Active).ToList();
        return Task.FromResult(list);
    }

    public Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var borrowing = _borrowings.FirstOrDefault(b => b.Id == id);
        return Task.FromResult(borrowing);
    }

    public Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Borrowing>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Borrowing> list = _borrowings
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ThenBy(b => b.Id)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Borrowing>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Borrowing> list = _borrowings
            .Where(b => b.EquipmentId == equipmentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ThenBy(b => b.Id)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        BorrowingSortField sortBy,
        bool ascending,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Borrowing> query = _borrowings;

        if (statusFilter.HasValue)
            query = query.Where(b => b.Status == statusFilter.Value);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(b =>
                (b.Student != null && (b.Student.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                       b.Student.StudentNumber.Contains(term, StringComparison.OrdinalIgnoreCase))) ||
                (b.Equipment != null && b.Equipment.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        query = (sortBy, ascending) switch
        {
            (BorrowingSortField.CheckoutDate, true) => query.OrderBy(b => b.DateBorrowed).ThenBy(b => b.Id),
            (BorrowingSortField.CheckoutDate, false) => query.OrderByDescending(b => b.DateBorrowed).ThenBy(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, true) => query.OrderBy(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, false) => query.OrderByDescending(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            _ => query.OrderByDescending(b => b.DateBorrowed).ThenBy(b => b.Id)
        };

        IReadOnlyList<Borrowing> list = query.ToList();
        return Task.FromResult(list);
    }

    public async Task<IReadOnlyList<Borrowing>> CheckoutItemsAsync(
        int studentId,
        IReadOnlyList<int> equipmentIds,
        DateTime checkoutDate,
        DateTime expectedReturnDate,
        Func<Task>? beforeCommitHook = null,
        CancellationToken cancellationToken = default)
    {
        if (_studentRepository is not null)
        {
            var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken)
                ?? throw new InvalidOperationException("Selected student does not exist.");

            if (!student.IsAllowedToBorrow)
                throw new InvalidOperationException("Selected student is restricted from borrowing.");
        }

        var activeCount = _borrowings.Count(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active);
        if (activeCount + equipmentIds.Count > 3)
            throw new InvalidOperationException($"Borrowing limit exceeded. Student currently has {activeCount} active item(s); maximum is 3.");

        var distinctIds = equipmentIds.Distinct().ToList();
        if (distinctIds.Count != equipmentIds.Count)
            throw new InvalidOperationException("Duplicate equipment items are not permitted in a single checkout.");

        var stagedEquipment = new List<Equipment>();
        if (_equipmentRepository is not null)
        {
            var items = await _equipmentRepository.GetByIdsAsync(distinctIds, cancellationToken);
            if (items.Count != distinctIds.Count)
                throw new InvalidOperationException("One or more selected equipment items no longer exist.");

            if (items.Any(e => !e.IsAvailable))
                throw new InvalidOperationException("One or more selected equipment items are currently unavailable.");

            stagedEquipment.AddRange(items);
        }

        if (_borrowings.Any(b => distinctIds.Contains(b.EquipmentId) && b.Status == BorrowingStatus.Active))
            throw new InvalidOperationException("One or more selected equipment items already have an active borrowing.");

        var nextId = _borrowings.Count == 0 ? 1 : _borrowings.Max(b => b.Id) + 1;
        var created = new List<Borrowing>(distinctIds.Count);
        foreach (var eqId in distinctIds)
        {
            created.Add(new Borrowing(nextId++, studentId, eqId, checkoutDate, expectedReturnDate));
        }

        if (beforeCommitHook is not null)
            await beforeCommitHook();

        foreach (var eq in stagedEquipment)
        {
            eq.MarkBorrowed();
            await _equipmentRepository!.UpdateAsync(eq, cancellationToken);
        }

        _borrowings.AddRange(created);
        return created;
    }

    public async Task ReturnBorrowingAsync(
        int borrowingId,
        DateTime returnDate,
        CancellationToken cancellationToken = default)
    {
        var borrowing = _borrowings.FirstOrDefault(b => b.Id == borrowingId)
            ?? throw new InvalidOperationException("Selected borrowing record does not exist.");

        if (borrowing.Status != BorrowingStatus.Active)
            throw new InvalidOperationException("Selected borrowing has already been returned.");

        borrowing.MarkReturned(returnDate);

        if (_equipmentRepository is not null)
        {
            var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId, cancellationToken);
            if (equipment is not null)
            {
                equipment.MarkReturned();
                await _equipmentRepository.UpdateAsync(equipment, cancellationToken);
            }
        }
    }
}