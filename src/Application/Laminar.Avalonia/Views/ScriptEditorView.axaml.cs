using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Laminar.Avalonia.Views;

public partial class ScriptEditorView : UserControl
{
    public ScriptEditorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
