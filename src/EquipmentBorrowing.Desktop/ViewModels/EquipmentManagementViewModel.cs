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

public partial class EquipmentManagementViewModel : ObservableObject
{
    private readonly EquipmentManagementService _equipmentService;
    private readonly BorrowingQueryService _queryService;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _historyCts;

    public ObservableCollection<Equipment> EquipmentItems { get; } = new();
    public ObservableCollection<BorrowingDisplayItem> EquipmentHistory { get; } = new();

    [ObservableProperty] private Equipment? _selectedEquipment;
    [ObservableProperty] private string _newIdentifier = string.Empty;
    [ObservableProperty] private string _newName = string.Empty;
    [ObservableProperty] private string _newDescription = string.Empty;

    [ObservableProperty] private string _editIdentifier = string.Empty;
    [ObservableProperty] private string _editName = string.Empty;
    [ObservableProperty] private string _editDescription = string.Empty;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isHistoryLoading;
    [ObservableProperty] private string _historyEmptyMessage = "Select an equipment item to view borrowing history.";
    [ObservableProperty] private string _historyErrorMessage = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isSuccess;

    public EquipmentManagementViewModel(
        EquipmentManagementService equipmentService,
        BorrowingQueryService queryService)
    {
        _equipmentService = equipmentService;
        _queryService = queryService;
    }

    partial void OnSelectedEquipmentChanged(Equipment? value)
    {
        if (value is null)
        {
            _historyCts?.Cancel();
            EditIdentifier = string.Empty;
            EditName = string.Empty;
            EditDescription = string.Empty;
            EquipmentHistory.Clear();
            HistoryErrorMessage = string.Empty;
            HistoryEmptyMessage = "Select an equipment item to view borrowing history.";
            return;
        }

        EditIdentifier = value.Identifier;
        EditName = value.Name;
        EditDescription = value.Description ?? string.Empty;
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
            var selectedId = SelectedEquipment?.Id;
            var items = await _queryService.GetEquipmentCatalogAsync(token);

            if (token.IsCancellationRequested)
                return;

            EquipmentItems.Clear();
            foreach (var e in items) EquipmentItems.Add(e);

            if (selectedId.HasValue)
                SelectedEquipment = EquipmentItems.FirstOrDefault(e => e.Id == selectedId.Value);
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer refresh.
        }
        catch
        {
            EquipmentItems.Clear();
            IsSuccess = false;
            StatusMessage = "Unable to load equipment from PostgreSQL. Check connection and retry.";
        }
        finally
        {
            if (!token.IsCancellationRequested)
                IsLoading = false;
        }
    }

    private async Task LoadHistoryAsync(int equipmentId)
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
            var history = await _queryService.GetEquipmentHistoryAsync(equipmentId, token);

            if (token.IsCancellationRequested)
                return;

            EquipmentHistory.Clear();
            foreach (var b in history)
                EquipmentHistory.Add(BorrowingDisplayItem.FromEntity(b));

            HistoryEmptyMessage = EquipmentHistory.Count == 0
                ? "This equipment item has no active or returned borrowing history."
                : string.Empty;
        }
        catch (OperationCanceledException)
        {
            // Superseded by selecting another item.
        }
        catch (InvalidOperationException ex)
        {
            EquipmentHistory.Clear();
            IsSuccess = false;
            HistoryErrorMessage = ex.Message;
            StatusMessage = ex.Message;
        }
        catch
        {
            EquipmentHistory.Clear();
            IsSuccess = false;
            const string msg = "Unable to load equipment borrowing history from PostgreSQL. Check connection and retry.";
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
    private async Task RegisterEquipmentAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _equipmentService.RegisterAsync(NewIdentifier, NewName, NewDescription);
            IsSuccess = result.Success;
            StatusMessage = result.Message;

            if (result.Success)
            {
                NewIdentifier = string.Empty;
                NewName = string.Empty;
                NewDescription = string.Empty;
                await LoadAsync();
                if (result.Equipment is not null)
                    SelectedEquipment = EquipmentItems.FirstOrDefault(e => e.Id == result.Equipment.Id);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateEquipmentAsync()
    {
        if (SelectedEquipment is null)
        {
            IsSuccess = false;
            StatusMessage = "Select an equipment item to update.";
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _equipmentService.UpdateAsync(
                SelectedEquipment.Id,
                EditIdentifier,
                EditName,
                EditDescription);

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