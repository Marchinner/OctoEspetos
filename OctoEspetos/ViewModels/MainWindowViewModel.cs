using System;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OctoEspetos.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty]
    private ViewModelBase _currentView;

    public MainWindowViewModel()
    {
        // Start the app on the QuickOrder screen (or your Home screen)
        CurrentView = new QuickOrderViewModel();
    }

    // Method to navigate (You can bind buttons to this)
    [RelayCommand]
    public void GoToHome()
    {
        // CurrentView = new HomeViewModel(); // Assuming you create a HomeViewModel later
    }

    [RelayCommand]
    public void NavigateToDashboard()
    {
        var vm = new OrdersDashboardViewModel();
        vm.RequestGoBack = () => GoToOrder(); // Define the "Back" action
        CurrentView = vm;
    }

    [RelayCommand]
    public void GoToOrder()
    {
        var orderVm = new QuickOrderViewModel();

        // When order is done, go back to Home (or refresh)
        orderVm.OnFinished = () =>
        {
            // GoToHome(); // Switch view back
            // For now, just reset the order screen:
            GoToOrder();
        };

        CurrentView = orderVm;
    }

    [RelayCommand]
        public void Exit()
        {
            // Clean way to close Avalonia App
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
