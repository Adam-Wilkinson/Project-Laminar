using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Laminar.Contracts.Scripting;

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
