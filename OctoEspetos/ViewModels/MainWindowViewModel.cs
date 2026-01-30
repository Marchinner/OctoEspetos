using System;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OctoEspetos.Models;

namespace OctoEspetos.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty]
    private ViewModelBase _currentView;

    public MainWindowViewModel()
    {
        // Start the app on the QuickOrder screen
        CurrentView = new QuickOrderViewModel();
    }

    [RelayCommand]
    public void GoToHome()
    {
        // CurrentView = new HomeViewModel();
    }

    [RelayCommand]
    public void NavigateToDashboard()
    {
        var vm = new OrdersDashboardViewModel();
        vm.RequestGoBack = () => GoToOrder();
        vm.RequestEditOrder = (order) => EditOrder(order);
        CurrentView = vm;
    }

    [RelayCommand]
    public void GoToOrder()
    {
        var orderVm = new QuickOrderViewModel();

        orderVm.OnFinished = () =>
        {
            GoToOrder();
        };

        CurrentView = orderVm;
    }

    public void EditOrder(Order order)
    {
        var orderVm = new QuickOrderViewModel();
        orderVm.LoadOrder(order);
        orderVm.OnFinished = () => NavigateToDashboard();
        CurrentView = orderVm;
    }

    [RelayCommand]
    public void Exit()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
        else
        {
            Environment.Exit(0);
        }
    }
}
