using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public new static App? Current => (App?)Avalonia.Application.Current;
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        // Repositories
        collection.AddSingleton<IStudentRepository, InMemoryStudentRepository>();
        collection.AddSingleton<IEquipmentRepository, InMemoryEquipmentRepository>();
        collection.AddSingleton<IBorrowingRepository, InMemoryBorrowingRepository>();

        // Services
        collection.AddTransient<BorrowEquipmentService>();
        collection.AddTransient<ReturnEquipmentService>();

        // ViewModels
        collection.AddTransient<EquipmentViewModel>();
        collection.AddTransient<BorrowingsViewModel>();
        collection.AddTransient<MainWindowViewModel>();

        Services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {

            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}