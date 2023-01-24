using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Laminar.PluginFramework.UserInterfaces;

namespace BasicFunctionality.Avalonia.UserControls;

public class StringDisplay : UserControl
{
    readonly TextBlock _mainTextBlock;
    private IDisplayValue _displayValue;
    private StringViewer _interfaceDefinition;

    public StringDisplay()
    {
        InitializeComponent();

        _mainTextBlock = this.FindControl<TextBlock>("PART_MainDisplay");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        DataContextChanged += StringDisplay_DataContextChanged;
    }

    private void StringDisplay_DataContextChanged(object sender, System.EventArgs e)
    {
        if (DataContext is IDisplayValue displayValue)
        {
            _displayValue = displayValue;
            _interfaceDefinition = displayValue.InterfaceDefinition as StringViewer;
            displayValue.PropertyChanged += DisplayValue_PropertyChanged;
            DisplayValue_PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IDisplayValue.Value)));
        }
    }

    private void DisplayValue_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDisplayValue.Value))
        {
            if (_displayValue.Value.ToString().Length > _interfaceDefinition.MaxStringLength)
            {
                _mainTextBlock.Text = _displayValue.Value.ToString().Substring(0, _interfaceDefinition.MaxStringLength) + "...";
            }
            else
            {
                _mainTextBlock.Text = _displayValue.Value.ToString();
            }
        }
    }
}
