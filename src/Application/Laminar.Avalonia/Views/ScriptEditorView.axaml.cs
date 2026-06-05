using Avalonia.Controls;
using Laminar.Avalonia.ViewModels;

namespace Laminar.Avalonia.Views;

public partial class ScriptEditorView : UserControl
{
    public ScriptEditorView()
    {
        UpdateNodeTree();
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e) => UpdateNodeTree();

    private void UpdateNodeTree()
    {
        Resources["NodeTree"] = (DataContext as ScriptEditorViewModel)?.NodeTree;   
    }
}