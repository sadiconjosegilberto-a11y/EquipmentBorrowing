using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public class BorrowingDisplayItem
{
    public int BorrowingId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public DateTime DateBorrowed { get; init; }
    public DateTime ExpectedReturnDate { get; init; }
}

public partial class BorrowingsViewModel : ObservableObject
{
    private readonly ReturnEquipmentService _returnService;
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IStudentRepository _studentRepository;

    public ObservableCollection<BorrowingDisplayItem> ActiveBorrowings { get; } = new();

    [ObservableProperty] private BorrowingDisplayItem? _selectedBorrowing;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isSuccess;

    public BorrowingsViewModel(
        ReturnEquipmentService returnService,
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository,
        IStudentRepository studentRepository)
    {
        _returnService = returnService;
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
        _studentRepository = studentRepository;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        ActiveBorrowings.Clear();
        var borrowings = await _borrowingRepository.GetAllActiveAsync();

        foreach (var b in borrowings)
        {
            var equipment = await _equipmentRepository.GetByIdAsync(b.EquipmentId);
            var student = await _studentRepository.GetByIdAsync(b.StudentId);

            ActiveBorrowings.Add(new BorrowingDisplayItem
            {
                BorrowingId = b.Id,
                StudentName = student?.Name ?? $"Student #{b.StudentId}",
                EquipmentName = equipment?.Name ?? $"Equipment #{b.EquipmentId}",
                DateBorrowed = b.DateBorrowed,
                ExpectedReturnDate = b.ExpectedReturnDate
            });
        }
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        if (SelectedBorrowing is null)
        {
            StatusMessage = "Please select a borrowing to return.";
            IsSuccess = false;
            return;
        }

        var result = await _returnService.ExecuteAsync(SelectedBorrowing.BorrowingId);

        IsSuccess = result.Success;
        StatusMessage = result.Success
            ? "Equipment returned successfully."
            : result.FailureReason ?? "An error occurred.";

        if (result.Success)
            await LoadAsync();
    }
}
