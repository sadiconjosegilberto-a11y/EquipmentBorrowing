using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly BorrowingsViewModel _borrowingsViewModel;

    [ObservableProperty] private ObservableObject _currentView;

    public MainWindowViewModel(
        EquipmentViewModel equipmentViewModel,
        BorrowingsViewModel borrowingsViewModel)
    {
        _equipmentViewModel = equipmentViewModel;
        _borrowingsViewModel = borrowingsViewModel;
        _currentView = equipmentViewModel;

        // Load initial data
        _ = equipmentViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task NavigateToEquipmentAsync()
    {
        await _equipmentViewModel.LoadAsync();
        CurrentView = _equipmentViewModel;
    }

    [RelayCommand]
    private async Task NavigateToBorrowingsAsync()
    {
        await _borrowingsViewModel.LoadAsync();
        CurrentView = _borrowingsViewModel;
    }
}
