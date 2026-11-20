using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public record StudentOperationResult(bool Success, string Message, Student? Student = null, bool IsUnknownOutcome = false);

public class StudentManagementService
{
    private readonly IStudentRepository _studentRepository;

    public StudentManagementService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<StudentOperationResult> RegisterAsync(
        string studentNumber,
        string name,
        string? contactInfo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            return new StudentOperationResult(false, "Institutional student number is required.");

        if (string.IsNullOrWhiteSpace(name))
            return new StudentOperationResult(false, "Student full name is required.");

        var normalized = studentNumber.Trim().ToUpperInvariant();

        try
        {
            if (await _studentRepository.ExistsByNormalizedNumberAsync(normalized, null, cancellationToken))
                return new StudentOperationResult(false, $"Student number '{studentNumber.Trim()}' is already registered.");

            var student = new Student(studentNumber, name, contactInfo);
            await _studentRepository.AddAsync(student, cancellationToken);
            return new StudentOperationResult(true, "Student registered successfully.", student);
        }
        catch (ArgumentException ex)
        {
            return new StudentOperationResult(false, ex.Message);
        }
        catch (Exception ex)
        {
            var (unknown, msg) = PersistenceErrorHelper.Describe(ex);
            return new StudentOperationResult(false, msg, null, unknown);
        }
    }

    public async Task<StudentOperationResult> UpdateAsync(
        int studentId,
        string studentNumber,
        string name,
        string? contactInfo,
        CancellationToken cancellationToken = default)
    {
        if (studentId <= 0)
            return new StudentOperationResult(false, "Select an existing student to update.");

        if (string.IsNullOrWhiteSpace(studentNumber))
            return new StudentOperationResult(false, "Institutional student number is required.");

        if (string.IsNullOrWhiteSpace(name))
            return new StudentOperationResult(false, "Student full name is required.");

        var normalized = studentNumber.Trim().ToUpperInvariant();

        try
        {
            var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
                return new StudentOperationResult(false, "Selected student record no longer exists.");

            if (await _studentRepository.ExistsByNormalizedNumberAsync(normalized, studentId, cancellationToken))
                return new StudentOperationResult(false, $"Student number '{studentNumber.Trim()}' is already assigned to another student.");

            student.UpdateDetails(studentNumber, name, contactInfo);
            await _studentRepository.UpdateAsync(student, cancellationToken);
            return new StudentOperationResult(true, "Student information updated.", student);
        }
        catch (ArgumentException ex)
        {
            return new StudentOperationResult(false, ex.Message);
        }
        catch (Exception ex)
        {
            var (unknown, msg) = PersistenceErrorHelper.Describe(ex);
            return new StudentOperationResult(false, msg, null, unknown);
        }
    }

    public async Task<StudentOperationResult> ChangeEligibilityAsync(
        int studentId,
        bool allowBorrowing,
        CancellationToken cancellationToken = default)
    {
        if (studentId <= 0)
            return new StudentOperationResult(false, "Select an existing student first.");

        try
        {
            var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
                return new StudentOperationResult(false, "Selected student record no longer exists.");

            var changed = allowBorrowing ? student.PermitBorrowing() : student.RestrictBorrowing();
            if (!changed)
            {
                var unchangedMsg = allowBorrowing
                    ? "Student is already permitted to borrow."
                    : "Student is already restricted from borrowing.";
                return new StudentOperationResult(true, unchangedMsg, student);
            }

            await _studentRepository.UpdateAsync(student, cancellationToken);
            var message = allowBorrowing
                ? "Student borrowing privileges permitted."
                : "Student borrowing privileges restricted. Existing active borrowings remain active until returned.";
            return new StudentOperationResult(true, message, student);
        }
        catch (Exception ex)
        {
            var (unknown, msg) = PersistenceErrorHelper.Describe(ex);
            return new StudentOperationResult(false, msg, null, unknown);
        }
    }
}