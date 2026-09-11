using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ObservableObject
{
    private readonly BorrowEquipmentService _borrowService;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IStudentRepository _studentRepository;

    public ObservableCollection<Equipment> EquipmentItems { get; } = new();
    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty] private Equipment? _selectedEquipment;
    [ObservableProperty] private Student? _selectedStudent;
    [ObservableProperty] private DateTime? _expectedReturnDate = DateTime.Today.AddDays(7);
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isSuccess;

    public EquipmentViewModel(
        BorrowEquipmentService borrowService,
        IEquipmentRepository equipmentRepository,
        IStudentRepository studentRepository)
    {
        _borrowService = borrowService;
        _equipmentRepository = equipmentRepository;
        _studentRepository = studentRepository;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        EquipmentItems.Clear();
        Students.Clear();

        var equipment = await _equipmentRepository.GetAllAsync();
        foreach (var e in equipment) EquipmentItems.Add(e);

        var students = await _studentRepository.GetAllAsync();
        foreach (var s in students) Students.Add(s);
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student.";
            IsSuccess = false;
            return;
        }

        if (SelectedEquipment is null)
        {
            StatusMessage = "Please select an equipment item.";
            IsSuccess = false;
            return;
        }

        if (ExpectedReturnDate is null || ExpectedReturnDate.Value.Date <= DateTime.Today)
        {
            StatusMessage = "Expected return date must be in the future.";
            IsSuccess = false;
            return;
        }

        var result = await _borrowService.ExecuteAsync(
            SelectedStudent.Id,
            SelectedEquipment.Id,
            ExpectedReturnDate.Value);

        IsSuccess = result.Success;
        StatusMessage = result.Success
            ? $"Borrowing #{result.Borrowing!.Id} created successfully."
            : result.FailureReason ?? "An error occurred.";

        if (result.Success)
            await LoadAsync();
    }
}
