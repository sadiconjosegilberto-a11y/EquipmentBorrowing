namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAllowedToBorrow { get; private set; } = true;

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