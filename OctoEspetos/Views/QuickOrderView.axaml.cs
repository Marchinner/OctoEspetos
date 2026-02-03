using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using System;
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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is QuickOrderViewModel vm)
        {
            vm.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(QuickOrderViewModel.IsCartExpanded))
                {
                    UpdateLayoutState();
                }
            };
        }
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateLayoutState();
    }

    private void UpdateLayoutState()
    {
        if (DataContext is not QuickOrderViewModel vm) return;
        
        if (Bounds.Width > Bounds.Height)
        {
            // Landscape
            ContentGrid.ColumnDefinitions.Clear();
            ContentGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            ContentGrid.ColumnDefinitions.Add(new ColumnDefinition(300, GridUnitType.Pixel));

            ContentGrid.RowDefinitions.Clear();
            ContentGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            Grid.SetColumn(ProductArea, 0);
            Grid.SetRow(ProductArea, 0);

            Grid.SetColumn(CartArea, 1);
            Grid.SetRow(CartArea, 0);
            
            CartArea.BorderThickness = new Thickness(1, 0, 0, 0);
        }
        else
        {
            // Portrait
            ContentGrid.ColumnDefinitions.Clear();
            ContentGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            ContentGrid.RowDefinitions.Clear();
            if (vm.IsCartExpanded)
            {
                // Expandido: Prioridade para o Carrinho (Produtos 40%, Carrinho 60%)
                // Isso permite ver mais itens na lista
                ContentGrid.RowDefinitions.Add(new RowDefinition(0.8, GridUnitType.Star));
                ContentGrid.RowDefinitions.Add(new RowDefinition(1.2, GridUnitType.Star));
            }
            else
            {
                // Colapsado: Altura fixa segura para mostrar apenas Cabeçalho e Rodapé
                // "Auto" pode causar problemas se o conteúdo interno demorar para atualizar a visibilidade
                ContentGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                ContentGrid.RowDefinitions.Add(new RowDefinition(170, GridUnitType.Pixel));
            }

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