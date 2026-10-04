namespace EquipmentBorrowing.Domain;

public class Equipment
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; } = true;

    // Parameterless constructor for EF Core materialization
    private Equipment() { }

    public Equipment(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public void MarkBorrowed() => IsAvailable = false;
    public void MarkReturned() => IsAvailable = true;
}