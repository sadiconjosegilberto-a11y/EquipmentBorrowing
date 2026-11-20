namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    public int Id { get; private set; }
    public int StudentId { get; private set; }
    public int EquipmentId { get; private set; }
    public DateTime DateBorrowed { get; private set; }
    public DateTime ExpectedReturnDate { get; private set; }
    public DateTime? ReturnedDate { get; private set; }
    public BorrowingStatus Status { get; private set; }

    public Student? Student { get; private set; }
    public Equipment? Equipment { get; private set; }

    private Borrowing() { }

    public Borrowing(int studentId, int equipmentId, DateTime dateBorrowed, DateTime expectedReturnDate)
        : this(0, studentId, equipmentId, dateBorrowed, expectedReturnDate)
    {
    }

    public Borrowing(int id, int studentId, int equipmentId, DateTime dateBorrowed, DateTime expectedReturnDate)
    {
        if (expectedReturnDate.Date <= dateBorrowed.Date)
            throw new ArgumentException("Expected return date must be later than checkout date.", nameof(expectedReturnDate));

        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed.Date;
        ExpectedReturnDate = expectedReturnDate.Date;
        Status = BorrowingStatus.Active;
    }

    public void MarkReturned() => MarkReturned(DateTime.Today);

    public void MarkReturned(DateTime returnDate)
    {
        if (Status != BorrowingStatus.Active)
            throw new InvalidOperationException("Only an active borrowing may be returned.");

        if (returnDate.Date < DateBorrowed.Date)
            throw new ArgumentException("Actual return date cannot precede checkout date.", nameof(returnDate));

        ReturnedDate = returnDate.Date;
        Status = BorrowingStatus.Returned;
    }
}