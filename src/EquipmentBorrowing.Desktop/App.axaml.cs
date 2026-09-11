using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    private IServiceProvider? _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Repositories as singletons so state persists across navigation
        var studentRepo = new InMemoryStudentRepository();
        studentRepo.Seed(new Student(1, "Alice Santos"));
        studentRepo.Seed(new Student(2, "Ben Cruz"));
        studentRepo.Seed(new Student(3, "Carla Reyes"));
        studentRepo.Seed(new Student(4, "Diego Lim"));
        studentRepo.Seed(new Student(5, "Eva Tan"));

        var equipmentRepo = new InMemoryEquipmentRepository();
        equipmentRepo.Seed(new Equipment(101, "Digital Multimeter"));
        equipmentRepo.Seed(new Equipment(102, "Oscilloscope"));
        equipmentRepo.Seed(new Equipment(103, "Function Generator"));
        equipmentRepo.Seed(new Equipment(104, "Soldering Station"));
        equipmentRepo.Seed(new Equipment(105, "Logic Analyzer"));
        equipmentRepo.Seed(new Equipment(106, "DC Power Supply"));
        equipmentRepo.Seed(new Equipment(107, "Breadboard Kit"));
        equipmentRepo.Seed(new Equipment(108, "Component Tester"));

        services.AddSingleton<IStudentRepository>(studentRepo);
        services.AddSingleton<IEquipmentRepository>(equipmentRepo);
        services.AddSingleton<IBorrowingRepository, InMemoryBorrowingRepository>();

        // Application services
        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();

        // ViewModels
        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<BorrowingsViewModel>();
        services.AddSingleton<MainWindowViewModel>();
    }
}
