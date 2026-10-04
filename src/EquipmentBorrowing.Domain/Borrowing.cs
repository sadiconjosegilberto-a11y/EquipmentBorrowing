namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    public int Id { get; private set; }
    public int StudentId { get; private set; }
    public int EquipmentId { get; private set; }
    public DateTime DateBorrowed { get; private set; }
    public DateTime ExpectedReturnDate { get; private set; }
    public BorrowingStatus Status { get; private set; }

    // Navigation properties for EF Core relational mapping
    public Student? Student { get; private set; }
    public Equipment? Equipment { get; private set; }

    // Parameterless constructor for EF Core materialization
    private Borrowing() { }

    public Borrowing(int id, int studentId, int equipmentId, DateTime dateBorrowed, DateTime expectedReturnDate)
    {
        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    public void MarkReturned() => Status = BorrowingStatus.Returned;
}