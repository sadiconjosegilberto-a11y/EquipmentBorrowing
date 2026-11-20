using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ObservableObject
{
    private readonly BorrowEquipmentService _borrowService;
    private readonly BorrowingQueryService _queryService;

    public ObservableCollection<Equipment> EquipmentItems { get; } = new();
    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<Equipment> CheckoutCart { get; } = new();

    [ObservableProperty] private Equipment? _selectedEquipment;
    [ObservableProperty] private Equipment? _selectedCartItem;
    [ObservableProperty] private Student? _selectedStudent;
    [ObservableProperty] private DateTime? _expectedReturnDate = DateTime.Today.AddDays(7);
    [ObservableProperty] private bool _showAvailableOnly;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _emptyMessage = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isSuccess;

    public EquipmentViewModel(
        BorrowEquipmentService borrowService,
        BorrowingQueryService queryService)
    {
        _borrowService = borrowService;
        _queryService = queryService;
    }

    partial void OnShowAvailableOnlyChanged(bool value) => _ = LoadAsync();

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        EmptyMessage = string.Empty;

        try
        {
            var equipment = ShowAvailableOnly
                ? await _queryService.GetAvailableEquipmentAsync()
                : await _queryService.GetEquipmentCatalogAsync();

            var students = await _queryService.GetAllStudentsAsync();

            EquipmentItems.Clear();
            foreach (var e in equipment) EquipmentItems.Add(e);

            var previousStudentId = SelectedStudent?.Id;
            Students.Clear();
            foreach (var s in students) Students.Add(s);
            if (previousStudentId.HasValue)
                SelectedStudent = Students.FirstOrDefault(s => s.Id == previousStudentId.Value);

            EmptyMessage = EquipmentItems.Count == 0
                ? (ShowAvailableOnly ? "No equipment items are currently available." : "No equipment registered in the catalog.")
                : string.Empty;
        }
        catch
        {
            EquipmentItems.Clear();
            IsSuccess = false;
            StatusMessage = "Unable to load catalog from PostgreSQL. Check connection and retry.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddToCheckout()
    {
        if (SelectedEquipment is null)
        {
            StatusMessage = "Select an equipment item to add to multi-item checkout.";
            IsSuccess = false;
            return;
        }

        if (CheckoutCart.Any(e => e.Id == SelectedEquipment.Id))
        {
            StatusMessage = $"'{SelectedEquipment.Name}' is already in the checkout list.";
            IsSuccess = false;
            return;
        }

        CheckoutCart.Add(SelectedEquipment);
        StatusMessage = $"Added '{SelectedEquipment.Name}' to multi-item checkout ({CheckoutCart.Count} item(s)).";
        IsSuccess = true;
    }

    [RelayCommand]
    private void RemoveFromCheckout()
    {
        if (SelectedCartItem is null) return;
        CheckoutCart.Remove(SelectedCartItem);
    }

    [RelayCommand]
    private void ClearCheckout() => CheckoutCart.Clear();

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
            StatusMessage = "Expected return date must be later than today.";
            IsSuccess = false;
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _borrowService.ExecuteAsync(
                SelectedStudent.Id,
                SelectedEquipment.Id,
                ExpectedReturnDate.Value);

            IsSuccess = result.Success;
            StatusMessage = result.Success
                ? $"Borrowing #{result.Borrowing!.Id} created successfully."
                : result.FailureReason ?? "Borrowing rejected.";

            if (result.Success)
                await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CheckoutMultipleAsync()
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student.";
            IsSuccess = false;
            return;
        }

        if (ExpectedReturnDate is null || ExpectedReturnDate.Value.Date <= DateTime.Today)
        {
            StatusMessage = "Expected return date must be later than today.";
            IsSuccess = false;
            return;
        }

        IsLoading = true;
        try
        {
            var ids = CheckoutCart.Select(e => e.Id).ToList();
            var result = await _borrowService.CheckoutMultipleAsync(
                SelectedStudent.Id,
                ids,
                ExpectedReturnDate.Value);

            IsSuccess = result.Success;
            StatusMessage = result.Success
                ? $"Checked out {result.Borrowings.Count} items atomically."
                : result.FailureReason ?? "Multi-item checkout rejected.";

            if (result.Success)
            {
                CheckoutCart.Clear();
                await LoadAsync();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}