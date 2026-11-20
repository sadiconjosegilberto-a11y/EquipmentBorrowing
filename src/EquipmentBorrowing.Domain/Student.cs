namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; private set; }
    public string StudentNumber { get; private set; } = string.Empty;
    public string NormalizedStudentNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? ContactInfo { get; private set; }
    public bool IsAllowedToBorrow { get; private set; } = true;

    private Student() { }

    public Student(string studentNumber, string name, string? contactInfo = null)
        : this(0, studentNumber, name, contactInfo)
    {
    }

    public Student(int id, string studentNumber, string name, string? contactInfo = null)
    {
        Id = id;
        UpdateDetails(studentNumber, name, contactInfo);
        IsAllowedToBorrow = true;
    }

    public Student(int id, string name)
        : this(id, $"STU-{id}", name, null)
    {
    }

    public void UpdateDetails(string studentNumber, string name, string? contactInfo)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            throw new ArgumentException("Institutional student number is required.", nameof(studentNumber));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student full name is required.", nameof(name));

        StudentNumber = studentNumber.Trim();
        NormalizedStudentNumber = StudentNumber.ToUpperInvariant();
        Name = name.Trim();
        ContactInfo = string.IsNullOrWhiteSpace(contactInfo) ? null : contactInfo.Trim();
    }

    public bool PermitBorrowing()
    {
        if (IsAllowedToBorrow) return false;
        IsAllowedToBorrow = true;
        return true;
    }

    public bool RestrictBorrowing()
    {
        if (!IsAllowedToBorrow) return false;
        IsAllowedToBorrow = false;
        return true;
    }

    public void SuspendBorrowingPrivileges() => RestrictBorrowing();
}
namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; private set; }
    public string StudentNumber { get; private set; } = string.Empty;
    public string NormalizedStudentNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? ContactInfo { get; private set; }
    public bool IsAllowedToBorrow { get; private set; } = true;

    private Student() { }

    public Student(string studentNumber, string name, string? contactInfo = null)
        : this(0, studentNumber, name, contactInfo)
    {
    }

    public Student(int id, string studentNumber, string name, string? contactInfo = null)
    {
        Id = id;
        UpdateDetails(studentNumber, name, contactInfo);
        IsAllowedToBorrow = true;
    }

    public Student(int id, string name)
        : this(id, $"STU-{id}", name, null)
    {
    }

    public void UpdateDetails(string studentNumber, string name, string? contactInfo)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            throw new ArgumentException("Institutional student number is required.", nameof(studentNumber));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student full name is required.", nameof(name));

        StudentNumber = studentNumber.Trim();
        NormalizedStudentNumber = StudentNumber.ToUpperInvariant();
        Name = name.Trim();
        ContactInfo = string.IsNullOrWhiteSpace(contactInfo) ? null : contactInfo.Trim();
    }

    public bool PermitBorrowing()
    {
        if (IsAllowedToBorrow) return false;
        IsAllowedToBorrow = true;
        return true;
    }

    public bool RestrictBorrowing()
    {
        if (!IsAllowedToBorrow) return false;
        IsAllowedToBorrow = false;
        return true;
    }

    public void SuspendBorrowingPrivileges() => RestrictBorrowing();
}