using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OctoEspetos.Views;

public partial class OrdersDashboardView : UserControl
{
    public OrdersDashboardView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateLayoutState();
    }

    private void UpdateLayoutState()
    {
        if (HeaderGrid == null || OpenOrdersWrapPanel == null) return;

        bool isPortrait = Bounds.Height > Bounds.Width;

        if (isPortrait)
        {
            // --- PORTRAIT MODE ---
            
            // 1. Header: Empilhar Faturamento abaixo dos botões
            // Redefinir colunas e linhas se necessário, ou apenas mover elementos
            // Grid: Col 0 (Voltar), Col 1 (Espaço), Col 2 (Revenue), Col 3 (Atualizar)
            
            // Vamos simplificar: Botões na linha 0, Faturamento na linha 1
            Grid.SetRow(RevenuePanel, 1);
            Grid.SetColumn(RevenuePanel, 0);
            Grid.SetColumnSpan(RevenuePanel, 4);
            RevenuePanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            RevenuePanel.Margin = new Thickness(0, 15, 0, 0); // Mais espaço em cima

            // 2. Open Orders: List View (Full Width)
            // Subtrair margins (Padding do Border 15 + TabMargin 10 = ~30 a 50px)
            OpenOrdersWrapPanel.ItemWidth = Bounds.Width - 60;
            OpenOrdersWrapPanel.ItemHeight = 210; // Altura maior para acomodar fontes grandes
        }
        else
        {
            // --- LANDSCAPE MODE ---
            
            // 1. Header: Tudo na linha 0
            Grid.SetRow(RevenuePanel, 0);
            Grid.SetColumn(RevenuePanel, 2);
            Grid.SetColumnSpan(RevenuePanel, 1);
            RevenuePanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            RevenuePanel.Margin = new Thickness(0);

            // 2. Open Orders: Cards (220px)
            OpenOrdersWrapPanel.ItemWidth = 220;
            OpenOrdersWrapPanel.ItemHeight = 210; // Altura maior mesmo em landscape
        }
    }
}