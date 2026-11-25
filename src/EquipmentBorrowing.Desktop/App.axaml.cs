using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
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
        var connectionString = Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_CONNECTION")
            ?? throw new InvalidOperationException(
                "Set the EQUIPMENT_BORROWING_CONNECTION environment variable.");

        services.AddDbContextFactory<EquipmentBorrowingDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddSingleton<IDatabaseInitializer, PostgresDatabaseInitializer>();

        services.AddTransient<IStudentRepository, EfStudentRepository>();
        services.AddTransient<IEquipmentRepository, EfEquipmentRepository>();
        services.AddTransient<IBorrowingRepository, EfBorrowingRepository>();

        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();
        services.AddTransient<StudentManagementService>();
        services.AddTransient<EquipmentManagementService>();
        services.AddTransient<BorrowingQueryService>();

        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<BorrowingsViewModel>();
        services.AddTransient<StudentsViewModel>();
        services.AddTransient<EquipmentManagementViewModel>();
        services.AddSingleton<MainWindowViewModel>();
    }
}