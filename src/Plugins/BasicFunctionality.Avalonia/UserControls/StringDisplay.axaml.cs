using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace BasicFunctionality.Avalonia.UserControls;

public partial class StringDisplay : UserControl
{
    private InterfaceData<StringViewer, string>? _interface;
    
    public StringDisplay()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_interface is not null)
        {
            _interface.PropertyChanged -= DisplayValue_PropertyChanged;   
        }
        
        _interface = DataContext as InterfaceData<StringViewer, string>;
        if (_interface is not null)
        {
            _interface.PropertyChanged += DisplayValue_PropertyChanged;
        }
        
        DisplayValue_PropertyChanged(this, new PropertyChangedEventArgs(nameof(StringViewer.IXamlTarget.Value)));
    }

    private void DisplayValue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IDisplayValue.Value) || _interface is null) return;
        
        if (_interface.Value.Length > _interface.Definition.MaxStringLength)
        {
            ValueViewer.Text = _interface.Value[.._interface.Definition.MaxStringLength] + "...";
        }
        else
        {
            ValueViewer.Text = _interface.Value;
        }
    }
}
