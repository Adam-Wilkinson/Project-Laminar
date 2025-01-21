using Avalonia.Controls;
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
        if (DataContext is not IInterfaceData { Definition: EnumDropdown enumDropdown } interfaceData)
        {
            return;
        }

        if (enumDropdown.DropdownOptions is not null)
        {
            CBox.ItemsSource = enumDropdown.DropdownOptions;
        }

        if (interfaceData.Value.GetType().IsEnum)
        {
            CBox.ItemsSource = interfaceData.Value.GetType().GetEnumValues();
        }
    }
}