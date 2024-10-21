using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Laminar.Avalonia.Views;

public class ScriptEditor : UserControl
{
    public ScriptEditor()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
