using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using OctoEspetos.ViewModels;

namespace OctoEspetos.Views;

public partial class QuickOrderView : UserControl
{
    public QuickOrderView()
    {
        InitializeComponent();
        
        // Registrar handler de teclado e layout
        KeyDown += OnKeyDown;
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (Bounds.Width > Bounds.Height)
        {
            // Landscape
            ContentGrid.ColumnDefinitions = new ColumnDefinitions("*, 300");
            ContentGrid.RowDefinitions = new RowDefinitions("*");

            Grid.SetColumn(ProductArea, 0);
            Grid.SetRow(ProductArea, 0);

            Grid.SetColumn(CartArea, 1);
            Grid.SetRow(CartArea, 0);
            
            CartArea.BorderThickness = new Thickness(1, 0, 0, 0);
        }
        else
        {
            // Portrait
            ContentGrid.ColumnDefinitions = new ColumnDefinitions("*");
            ContentGrid.RowDefinitions = new RowDefinitions("*, 400");

            Grid.SetColumn(ProductArea, 0);
            Grid.SetRow(ProductArea, 0);

            Grid.SetColumn(CartArea, 0);
            Grid.SetRow(CartArea, 1);
            
            CartArea.BorderThickness = new Thickness(0, 1, 0, 0);
        }
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