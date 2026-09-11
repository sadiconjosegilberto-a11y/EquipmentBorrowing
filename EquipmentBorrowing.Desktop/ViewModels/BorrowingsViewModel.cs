using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public class BorrowingDisplayModel
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public BorrowingStatus Status { get; set; }
}

public partial class BorrowingsViewModel : ObservableObject
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly ReturnEquipmentService _returnEquipmentService;

    [ObservableProperty]
    private ObservableCollection<BorrowingDisplayModel> activeBorrowings = new();

    [ObservableProperty]
    private BorrowingDisplayModel? selectedBorrowing;

    [ObservableProperty]
    private string? statusMessage;

    public BorrowingsViewModel(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository,
        ReturnEquipmentService returnEquipmentService)
    {
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
        _returnEquipmentService = returnEquipmentService;

        LoadActiveBorrowings();
    }

    public async void LoadActiveBorrowings()
    {
        var allBorrowings = await _borrowingRepository.GetAllAsync();
        var active = allBorrowings.Where(b => b.Status == BorrowingStatus.Active);

        var displayList = new ObservableCollection<BorrowingDisplayModel>();
        foreach (var borrowing in active)
        {
            var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId);
            displayList.Add(new BorrowingDisplayModel
            {
                Id = borrowing.Id,
                EquipmentId = borrowing.EquipmentId,
                EquipmentName = equipment?.Name ?? "Unknown",
                Status = borrowing.Status
            });
        }

        ActiveBorrowings = displayList;
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