using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class StudentsViewModel : ObservableObject
{
    private readonly StudentManagementService _studentService;
    private readonly BorrowingQueryService _queryService;

    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<BorrowingDisplayItem> StudentHistory { get; } = new();

    [ObservableProperty] private Student? _selectedStudent;
    [ObservableProperty] private string _newStudentNumber = string.Empty;
    [ObservableProperty] private string _newName = string.Empty;
    [ObservableProperty] private string _newContactInfo = string.Empty;

    [ObservableProperty] private string _editStudentNumber = string.Empty;
    [ObservableProperty] private string _editName = string.Empty;
    [ObservableProperty] private string _editContactInfo = string.Empty;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isHistoryLoading;
    [ObservableProperty] private string _historyEmptyMessage = "Select a student to view borrowing history.";
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isSuccess;

    public StudentsViewModel(
        StudentManagementService studentService,
        BorrowingQueryService queryService)
    {
        _studentService = studentService;
        _queryService = queryService;
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (value is null)
        {
            EditStudentNumber = string.Empty;
            EditName = string.Empty;
            EditContactInfo = string.Empty;
            StudentHistory.Clear();
            HistoryEmptyMessage = "Select a student to view borrowing history.";
            return;
        }

        EditStudentNumber = value.StudentNumber;
        EditName = value.Name;
        EditContactInfo = value.ContactInfo ?? string.Empty;
        _ = LoadHistoryAsync(value.Id);
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var selectedId = SelectedStudent?.Id;
            var students = await _queryService.GetAllStudentsAsync();

            Students.Clear();
            foreach (var s in students) Students.Add(s);

            if (selectedId.HasValue)
                SelectedStudent = Students.FirstOrDefault(s => s.Id == selectedId.Value);
        }
        catch
        {
            Students.Clear();
            IsSuccess = false;
            StatusMessage = "Unable to load students from PostgreSQL. Check connection and retry.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadHistoryAsync(int studentId)
    {
        IsHistoryLoading = true;
        HistoryEmptyMessage = string.Empty;
        try
        {
            var history = await _queryService.GetStudentHistoryAsync(studentId);
            StudentHistory.Clear();
            foreach (var b in history)
                StudentHistory.Add(BorrowingDisplayItem.FromEntity(b));

            HistoryEmptyMessage = StudentHistory.Count == 0
                ? "This student has no active or returned borrowing history."
                : string.Empty;
        }
        catch (System.Exception ex)
        {
            StudentHistory.Clear();
            HistoryEmptyMessage = ex.Message;
        }
        finally
        {
            IsHistoryLoading = false;
        }
    }

    [RelayCommand]
    private async Task RegisterStudentAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _studentService.RegisterAsync(NewStudentNumber, NewName, NewContactInfo);
            IsSuccess = result.Success;
            StatusMessage = result.Message;

            if (result.Success)
            {
                NewStudentNumber = string.Empty;
                NewName = string.Empty;
                NewContactInfo = string.Empty;
                await LoadAsync();
                if (result.Student is not null)
                    SelectedStudent = Students.FirstOrDefault(s => s.Id == result.Student.Id);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateStudentAsync()
    {
        if (SelectedStudent is null)
        {
            IsSuccess = false;
            StatusMessage = "Select a student to update.";
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _studentService.UpdateAsync(
                SelectedStudent.Id,
                EditStudentNumber,
                EditName,
                EditContactInfo);

            IsSuccess = result.Success;
            StatusMessage = result.Message;

            if (result.Success)
                await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PermitStudentAsync() => await SetEligibilityAsync(true);

    [RelayCommand]
    private async Task RestrictStudentAsync() => await SetEligibilityAsync(false);

    private async Task SetEligibilityAsync(bool allow)
    {
        if (SelectedStudent is null)
        {
            IsSuccess = false;
            StatusMessage = "Select a student first.";
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _studentService.ChangeEligibilityAsync(SelectedStudent.Id, allow);
            IsSuccess = result.Success;
            StatusMessage = result.Message;

            if (result.Success)
                await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }
}