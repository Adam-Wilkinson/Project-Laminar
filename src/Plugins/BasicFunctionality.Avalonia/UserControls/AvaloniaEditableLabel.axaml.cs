using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace BasicFunctionality.Avalonia.UserControls;

public partial class AvaloniaEditableLabel : UserControl
{
    private string? _persistentValue;

    public AvaloniaEditableLabel()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Display.DoubleTapped += AvaloniaEditableLabel_DoubleTapped;

        Editor.KeyDown += Entry_KeyDown;
    }

    private void Entry_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                SetText(Editor.Text);
                SetEditing(false);
                break;
            case Key.Escape:
                SetText(_persistentValue);
                SetEditing(false);
                break;
        }
    }

    private void AvaloniaEditableLabel_DoubleTapped(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _persistentValue = Display.Text;
        SetEditing(true);
        Editor.Focus();
        Editor.SelectAll();
    }

    private void SetEditing(bool editing)
    {
        if (DataContext is IDisplayValue { InterfaceDefinition: EditableLabel editableLabelDefinition })
        {
            editableLabelDefinition.IsBeingEdited = editing;
        }
    }

    private void SetText(string? text)
    {
        text ??= "";

        Display.Text = text;
        Editor.Text = text;
        if (DataContext is IDisplayValue displayValue)
        {
            displayValue.Value = text;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
