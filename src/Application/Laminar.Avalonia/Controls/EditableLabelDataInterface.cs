using System;
using Avalonia.Data;
using Avalonia.Layout;
using LabelInterfaceData = Laminar.PluginFramework.UserInterface.IInterfaceData<Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.EditableLabel, string>;

namespace Laminar.Avalonia.Controls;

public class EditableLabelDataInterface : EditableLabel
{
    static EditableLabelDataInterface()
    {
        HorizontalAlignmentProperty.OverrideDefaultValue<EditableLabelDataInterface>(HorizontalAlignment.Center);
    }
    
    protected override void OnDataContextEndUpdate()
    {
        if (DataContext is not LabelInterfaceData typedDataContext)
        {
            throw new InvalidCastException();
        }

        this[!TextProperty] = CompiledBinding.Create<LabelInterfaceData, string>(x => x.Value, mode: BindingMode.TwoWay);
        if (!string.IsNullOrWhiteSpace(typedDataContext.Name))
        {
            DisplayStringFormat = typedDataContext.Name + ": {0}";
        }
    }
}