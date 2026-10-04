namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsAllowedToBorrow { get; private set; } = true;

    // Parameterless constructor for EF Core materialization
    private Student() { }

    public Student(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public void SuspendBorrowingPrivileges()
    {
        IsAllowedToBorrow = false;
    }
}