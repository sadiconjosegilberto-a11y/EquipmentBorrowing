using EquipmentBorrowing.Domain;
using System;

namespace EquipmentBorrowing.Desktop.ViewModels;

public class BorrowingDisplayItem
{
    public int BorrowingId { get; init; }
    public string StudentNumber { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public string EquipmentIdentifier { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public DateTime DateBorrowed { get; init; }
    public DateTime ExpectedReturnDate { get; init; }
    public DateTime? ReturnedDate { get; init; }
    public BorrowingStatus Status { get; init; }
    public bool IsActive => Status == BorrowingStatus.Active;

    public static BorrowingDisplayItem FromEntity(Borrowing b) => new()
    {
        BorrowingId = b.Id,
        StudentNumber = b.Student?.StudentNumber ?? string.Empty,
        StudentName = b.Student?.Name ?? $"Student #{b.StudentId}",
        EquipmentIdentifier = b.Equipment?.Identifier ?? string.Empty,
        EquipmentName = b.Equipment?.Name ?? $"Equipment #{b.EquipmentId}",
        DateBorrowed = b.DateBorrowed,
        ExpectedReturnDate = b.ExpectedReturnDate,
        ReturnedDate = b.ReturnedDate,
        Status = b.Status
    };
}