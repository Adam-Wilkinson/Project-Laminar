using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace BasicFunctionality.Avalonia.UserControls;

public partial class AvaloniaEditableLabel : UserControl
{
    string? _persistentValue;
    readonly TextBlock _label;
    readonly TextBox _entry;

    public AvaloniaEditableLabel()
    {
        InitializeComponent();
        _label = this.FindControl<TextBlock>("PART_Display");
        _entry = this.FindControl<TextBox>("PART_Editor");

        _label.DoubleTapped += AvaloniaEditableLabel_DoubleTapped;

        _entry.KeyDown += Entry_KeyDown;
    }

    private void Entry_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SetText(_entry.Text);
            SetEditing(false);
        }

        if (e.Key == Key.Escape)
        {
            SetText(_persistentValue);
            SetEditing(false);
        }
    }

    private void AvaloniaEditableLabel_DoubleTapped(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _persistentValue = _label.Text;
        SetEditing(true);
        _entry.Focus();
        _entry.SelectAll();
    }

    private void SetEditing(bool editing)
    {
        if (DataContext is IDisplayValue displayValue && displayValue.InterfaceDefinition is EditableLabel editableLabelDefinition)
        {
            editableLabelDefinition.IsBeingEdited = editing;
        }
    }

    private void SetText(string? text)
    {
        text ??= "";

        _label.Text = text;
        _entry.Text = text;
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
