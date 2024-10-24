using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Laminar.Avalonia.Views;

public partial class HomepageView : UserControl
{
    public HomepageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
