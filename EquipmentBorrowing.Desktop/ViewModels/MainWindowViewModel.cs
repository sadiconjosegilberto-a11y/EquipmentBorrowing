using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly BorrowingsViewModel _borrowingsViewModel;

    [ObservableProperty]
    private object? currentViewModel;

    public MainWindowViewModel(EquipmentViewModel equipmentViewModel, BorrowingsViewModel borrowingsViewModel)
    {
        _equipmentViewModel = equipmentViewModel;
        _borrowingsViewModel = borrowingsViewModel;
        CurrentViewModel = _equipmentViewModel;
    }

    [RelayCommand]
    private void ShowEquipment()
    {
        CurrentViewModel = _equipmentViewModel;
    }

    [RelayCommand]
    private void ShowBorrowings()
    {
        _borrowingsViewModel.LoadActiveBorrowings();
        CurrentViewModel = _borrowingsViewModel;
    }
}