using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class StudentsViewModel : ObservableObject
{
    private readonly StudentManagementService _studentService;
    private readonly BorrowingQueryService _queryService;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _historyCts;

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
    [ObservableProperty] private string _historyErrorMessage = string.Empty;
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
            _historyCts?.Cancel();
            EditStudentNumber = string.Empty;
            EditName = string.Empty;
            EditContactInfo = string.Empty;
            StudentHistory.Clear();
            HistoryErrorMessage = string.Empty;
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
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        IsLoading = true;
        try
        {
            var selectedId = SelectedStudent?.Id;
            var students = await _queryService.GetAllStudentsAsync(token);

            if (token.IsCancellationRequested)
                return;

            Students.Clear();
            foreach (var s in students) Students.Add(s);

            if (selectedId.HasValue)
                SelectedStudent = Students.FirstOrDefault(s => s.Id == selectedId.Value);
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer refresh.
        }
        catch
        {
            Students.Clear();
            IsSuccess = false;
            StatusMessage = "Unable to load students from PostgreSQL. Check connection and retry.";
        }
        finally
        {
            if (!token.IsCancellationRequested)
                IsLoading = false;
        }
    }

    private async Task LoadHistoryAsync(int studentId)
    {
        _historyCts?.Cancel();
        _historyCts?.Dispose();
        _historyCts = new CancellationTokenSource();
        var token = _historyCts.Token;

        IsHistoryLoading = true;
        HistoryEmptyMessage = string.Empty;
        HistoryErrorMessage = string.Empty;

        try
        {
            var history = await _queryService.GetStudentHistoryAsync(studentId, token);

            if (token.IsCancellationRequested)
                return;

            StudentHistory.Clear();
            foreach (var b in history)
                StudentHistory.Add(BorrowingDisplayItem.FromEntity(b));

            HistoryEmptyMessage = StudentHistory.Count == 0
                ? "This student has no active or returned borrowing history."
                : string.Empty;
        }
        catch (OperationCanceledException)
        {
            // Superseded by selecting another student.
        }
        catch (InvalidOperationException ex)
        {
            StudentHistory.Clear();
            IsSuccess = false;
            HistoryErrorMessage = ex.Message;
            StatusMessage = ex.Message;
        }
        catch
        {
            StudentHistory.Clear();
            IsSuccess = false;
            const string msg = "Unable to load student borrowing history from PostgreSQL. Check connection and retry.";
            HistoryErrorMessage = msg;
            StatusMessage = msg;
        }
        finally
        {
            if (!token.IsCancellationRequested)
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