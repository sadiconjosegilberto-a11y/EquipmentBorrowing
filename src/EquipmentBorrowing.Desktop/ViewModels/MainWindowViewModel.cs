using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly BorrowingsViewModel _borrowingsViewModel;
    private readonly StudentsViewModel _studentsViewModel;
    private readonly EquipmentManagementViewModel _equipmentManagementViewModel;
    private bool _isDatabaseInitialized;

    [ObservableProperty] private ObservableObject _currentView;
    [ObservableProperty] private string _startupErrorMessage = string.Empty;

    public MainWindowViewModel(
        IDatabaseInitializer databaseInitializer,
        EquipmentViewModel equipmentViewModel,
        BorrowingsViewModel borrowingsViewModel,
        StudentsViewModel studentsViewModel,
        EquipmentManagementViewModel equipmentManagementViewModel)
    {
        _databaseInitializer = databaseInitializer;
        _equipmentViewModel = equipmentViewModel;
        _borrowingsViewModel = borrowingsViewModel;
        _studentsViewModel = studentsViewModel;
        _equipmentManagementViewModel = equipmentManagementViewModel;
        _currentView = equipmentViewModel;

        _ = InitializeAndLoadAsync();
    }

    [RelayCommand]
    private async Task InitializeAndLoadAsync()
    {
        if (!await EnsureDatabaseInitializedAsync())
            return;

        await _equipmentViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task NavigateToEquipmentAsync()
    {
        CurrentView = _equipmentViewModel;
        if (!await EnsureDatabaseInitializedAsync())
            return;

        await _equipmentViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task NavigateToBorrowingsAsync()
    {
        CurrentView = _borrowingsViewModel;
        if (!await EnsureDatabaseInitializedAsync())
            return;

        await _borrowingsViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task NavigateToStudentsAsync()
    {
        CurrentView = _studentsViewModel;
        if (!await EnsureDatabaseInitializedAsync())
            return;

        await _studentsViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task NavigateToEquipmentManagementAsync()
    {
        CurrentView = _equipmentManagementViewModel;
        if (!await EnsureDatabaseInitializedAsync())
            return;

        await _equipmentManagementViewModel.LoadAsync();
    }

    private async Task<bool> EnsureDatabaseInitializedAsync()
    {
        if (_isDatabaseInitialized)
            return true;

        try
        {
            await _databaseInitializer.InitializeAsync();
            _isDatabaseInitialized = true;
            StartupErrorMessage = string.Empty;
            return true;
        }
        catch
        {
            const string msg = "Unable to connect to or initialize PostgreSQL. Verify database availability and configuration, then retry.";
            StartupErrorMessage = msg;
            _equipmentViewModel.IsSuccess = false;
            _equipmentViewModel.StatusMessage = msg;
            _borrowingsViewModel.IsSuccess = false;
            _borrowingsViewModel.StatusMessage = msg;
            _studentsViewModel.IsSuccess = false;
            _studentsViewModel.StatusMessage = msg;
            _equipmentManagementViewModel.IsSuccess = false;
            _equipmentManagementViewModel.StatusMessage = msg;
            return false;
        }
    }
}