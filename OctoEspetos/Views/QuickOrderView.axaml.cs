using Avalonia.Controls;
using Avalonia.Input;
using OctoEspetos.ViewModels;

namespace OctoEspetos.Views;

public partial class QuickOrderView : UserControl
{
    public QuickOrderView()
    {
        InitializeComponent();
        
        // Registrar handler de teclado
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not QuickOrderViewModel vm) return;

        switch (e.Key)
        {
            case Key.F2:
                if (vm.ShowPaymentModalCommand.CanExecute(null))
                    vm.ShowPaymentModalCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.F3:
                if (vm.SaveOpenOrderCommand.CanExecute(null))
                    vm.SaveOpenOrderCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}