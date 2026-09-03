namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsAllowedToBorrow { get; set; } = true;
    public int ActiveBorrowingCount { get; set; }
    public int MaxActiveBorrowings { get; set; } = 3;
}