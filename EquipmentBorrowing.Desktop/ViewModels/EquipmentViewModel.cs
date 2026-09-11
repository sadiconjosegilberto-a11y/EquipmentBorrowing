using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ObservableObject
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly BorrowEquipmentService _borrowEquipmentService;

    [ObservableProperty]
    private ObservableCollection<Equipment> equipmentList = new();

    [ObservableProperty]
    private Equipment? selectedEquipment;

    [ObservableProperty]
    private string? statusMessage;

    public EquipmentViewModel(
        IEquipmentRepository equipmentRepository,
        BorrowEquipmentService borrowEquipmentService)
    {
        _equipmentRepository = equipmentRepository;
        _borrowEquipmentService = borrowEquipmentService;

        LoadEquipment();
    }

    private async void LoadEquipment()
    {
        var items = await _equipmentRepository.GetAllAsync();
        EquipmentList = new ObservableCollection<Equipment>(items);
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        if (SelectedEquipment == null)
        {
            StatusMessage = "Please select an equipment to borrow.";
            return;
        }

        // Hardcoded student ID 1 for demonstration. 
        // In a full system, this would come from a login or selection.
        bool success = await _borrowEquipmentService.BorrowAsync(1, SelectedEquipment.Id);

        if (success)
        {
            StatusMessage = "Equipment borrowed successfully!";
            LoadEquipment(); // Refresh the list
        }
        else
        {
            StatusMessage = "Failed to borrow equipment. It may be unavailable or you reached your limit.";
        }
    }
}