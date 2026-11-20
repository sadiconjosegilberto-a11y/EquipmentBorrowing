using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly BorrowingsViewModel _borrowingsViewModel;
    private readonly StudentsViewModel _studentsViewModel;
    private readonly EquipmentManagementViewModel _equipmentManagementViewModel;

    [ObservableProperty] private ObservableObject _currentView;

    public MainWindowViewModel(
        EquipmentViewModel equipmentViewModel,
        BorrowingsViewModel borrowingsViewModel,
        StudentsViewModel studentsViewModel,
        EquipmentManagementViewModel equipmentManagementViewModel)
    {
        _equipmentViewModel = equipmentViewModel;
        _borrowingsViewModel = borrowingsViewModel;
        _studentsViewModel = studentsViewModel;
        _equipmentManagementViewModel = equipmentManagementViewModel;
        _currentView = equipmentViewModel;

        _ = equipmentViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task NavigateToEquipmentAsync()
    {
        CurrentView = _equipmentViewModel;
        await _equipmentViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task NavigateToBorrowingsAsync()
    {
        CurrentView = _borrowingsViewModel;
        await _borrowingsViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task NavigateToStudentsAsync()
    {
        CurrentView = _studentsViewModel;
        await _studentsViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task NavigateToEquipmentManagementAsync()
    {
        CurrentView = _equipmentManagementViewModel;
        await _equipmentManagementViewModel.LoadAsync();
    }
}