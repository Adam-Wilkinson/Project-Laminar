using Avalonia.Controls;
using Avalonia.Data;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace BasicFunctionality.Avalonia.UserControls;

public partial class EnumEditor : UserControl
{
    public EnumEditor()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        const string bindingPath = $"{nameof(IInterfaceData.Definition)}.{nameof(EnumDropdown.DropdownOptions)}";
        
        if (DataContext is not IInterfaceData { Definition: EnumDropdown } interfaceData)
        {
            return;
        }
        
        CBox[!ItemsControl.ItemsSourceProperty] = 
            new Binding(bindingPath, BindingMode.OneWay)
            {
                FallbackValue = interfaceData.Value.GetType().IsEnum ? interfaceData.Value.GetType().GetEnumValues() : null,
            };
    }
}