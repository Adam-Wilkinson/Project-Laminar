using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Laminar.Avalonia.Controls;
public partial class NodePicker : UserControl
{
    public NodePicker()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
