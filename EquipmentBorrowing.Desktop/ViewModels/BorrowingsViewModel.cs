using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ObservableObject
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly ReturnEquipmentService _returnEquipmentService;

    [ObservableProperty]
    private ObservableCollection<Borrowing> activeBorrowings = new();

    [ObservableProperty]
    private Borrowing? selectedBorrowing;

    [ObservableProperty]
    private string? statusMessage;

    public BorrowingsViewModel(
        IBorrowingRepository borrowingRepository,
        ReturnEquipmentService returnEquipmentService)
    {
        _borrowingRepository = borrowingRepository;
        _returnEquipmentService = returnEquipmentService;

        LoadActiveBorrowings();
    }

    public async void LoadActiveBorrowings()
    {
        var allBorrowings = await _borrowingRepository.GetAllAsync();
        var active = allBorrowings.Where(b => b.Status == BorrowingStatus.Active);
        ActiveBorrowings = new ObservableCollection<Borrowing>(active);
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        if (SelectedBorrowing == null)
        {
            StatusMessage = "Please select a borrowing to return.";
            return;
        }

        bool success = await _returnEquipmentService.ReturnAsync(SelectedBorrowing.Id);

        if (success)
        {
            StatusMessage = "Equipment returned successfully.";
            LoadActiveBorrowings();
        }
        else
        {
            StatusMessage = "Failed to return equipment.";
        }
    }
}