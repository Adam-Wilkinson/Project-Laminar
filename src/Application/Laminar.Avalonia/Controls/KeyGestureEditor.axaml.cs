using System;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Avalonia.Controls;

public interface IKeyGestureEditorXamlTarget 
    : IInterfaceData<global::Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.KeyGestureEditor, KeyGesture>;

public partial class KeyGestureEditor : UserControl
{
    private static readonly IValueConverter NameToFormatStringConverter =
        new FuncValueConverter<string, string?>(name => string.IsNullOrWhiteSpace(name) ? null : name + ": {0}");
    
    public KeyGestureEditor()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is IInterfaceData)
        {
            KeyGestureInterface[!KeyGestureInterface.FormatStringProperty] = new Binding(nameof(IInterfaceData.Name))
            {
                Converter = NameToFormatStringConverter
            };
        }
    }
}