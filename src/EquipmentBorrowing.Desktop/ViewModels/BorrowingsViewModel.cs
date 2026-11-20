using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ObservableObject
{
    private readonly ReturnEquipmentService _returnService;
    private readonly BorrowingQueryService _queryService;

    public ObservableCollection<BorrowingDisplayItem> Borrowings { get; } = new();

    [ObservableProperty] private BorrowingDisplayItem? _selectedBorrowing;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private int _statusFilterIndex; // 0 = Active, 1 = Returned, 2 = All
    [ObservableProperty] private int _sortFieldIndex;    // 0 = Checkout Date, 1 = Expected Return Date
    [ObservableProperty] private bool _sortAscending;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _emptyMessage = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isSuccess;

    public BorrowingsViewModel(
        ReturnEquipmentService returnService,
        BorrowingQueryService queryService)
    {
        _returnService = returnService;
        _queryService = queryService;
    }

    partial void OnStatusFilterIndexChanged(int value) => _ = LoadAsync();
    partial void OnSortFieldIndexChanged(int value) => _ = LoadAsync();
    partial void OnSortAscendingChanged(bool value) => _ = LoadAsync();

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        EmptyMessage = string.Empty;

        try
        {
            BorrowingStatus? statusFilter = StatusFilterIndex switch
            {
                0 => BorrowingStatus.Active,
                1 => BorrowingStatus.Returned,
                _ => null
            };

            var sortField = SortFieldIndex == 1
                ? BorrowingSortField.ExpectedReturnDate
                : BorrowingSortField.CheckoutDate;

            var records = await _queryService.SearchBorrowingsAsync(
                SearchText,
                statusFilter,
                sortField,
                SortAscending);

            Borrowings.Clear();
            foreach (var b in records)
                Borrowings.Add(BorrowingDisplayItem.FromEntity(b));

            EmptyMessage = Borrowings.Count == 0
                ? "No borrowing records match the current criteria."
                : string.Empty;
        }
        catch
        {
            Borrowings.Clear();
            IsSuccess = false;
            StatusMessage = "Unable to retrieve borrowings from PostgreSQL. Check connection and retry.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        StatusFilterIndex = 0;
        SortFieldIndex = 0;
        SortAscending = false;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        if (SelectedBorrowing is null)
        {
            StatusMessage = "Please select an active borrowing to return.";
            IsSuccess = false;
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _returnService.ExecuteAsync(SelectedBorrowing.BorrowingId);

            IsSuccess = result.Success;
            StatusMessage = result.Success
                ? "Equipment returned successfully."
                : result.FailureReason ?? "Return rejected.";

            if (result.Success)
                await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }
}