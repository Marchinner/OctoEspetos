using Avalonia.Controls;

namespace OctoEspetos.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // Se a largura for pequena (ex: modo retrato < 700px)
        bool isSmall = Bounds.Width < 700;

        // Título: some no modo pequeno
        if (TitleText != null)
            TitleText.IsVisible = !isSmall;

        // Botões: texto curto no modo pequeno
        if (BtnOrder != null)
            BtnOrder.Content = isSmall ? "Novo" : "Novo Pedido";
            
        if (BtnDash != null)
            BtnDash.Content = isSmall ? "Histórico" : "Dashboard / Histórico";

        if (BtnInv != null)
            BtnInv.Content = isSmall ? "Inv." : "Inventário";
    }
}
