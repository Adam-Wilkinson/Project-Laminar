using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Domain;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;
using AvaloniaEditableLabel = Laminar.Avalonia.Controls.EditableLabel;
using AvaloniaKeyGestureEditor = Laminar.Avalonia.Controls.KeyGestureEditor;
using EditableLabelDefinition = Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.EditableLabel;
using KeyGestureEditorDefinition = Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.KeyGestureEditor;

namespace Laminar.Avalonia.InitializationTargets;

public class DataInterfaceRegistration(IDataInterfaceFactory interfaceFactory, ITypeInfoStore typeInfoStore) : IAfterApplicationBuiltTarget
{
    private static readonly IValueConverter NameToFormatStringConverter =
        new FuncValueConverter<string, string?>(name => string.IsNullOrWhiteSpace(name) ? null : name + ": {0}");
    
    public void OnApplicationBuilt()
    {
        typeInfoStore.RegisterType(typeof(KeyGesture),
            new TypeInfo("Key Gesture", new KeyGestureEditorDefinition(), new StringEditor(), "#FF5533", null));
        
        interfaceFactory.RegisterInterfaceFactory<KeyGestureEditorDefinition, KeyGesture, AvaloniaKeyGestureEditor>(() => new AvaloniaKeyGestureEditor
        {
            [!AvaloniaKeyGestureEditor.FormatStringProperty] = CompiledBinding.Create<IInterfaceData<KeyGesture>, string?>(x => x.Name, converter: NameToFormatStringConverter),
            [!AvaloniaKeyGestureEditor.GestureProperty] = CompiledBinding.Create<IInterfaceData<KeyGesture>, KeyGesture>(x => x.Value, mode: BindingMode.TwoWay)
        });
        
        interfaceFactory.RegisterInterfaceFactory<EditableLabelDefinition, string, AvaloniaEditableLabel>(() => new AvaloniaEditableLabel
        {
            [!AvaloniaEditableLabel.DisplayStringFormatProperty] = CompiledBinding.Create<IInterfaceData<string>, string>(x => x.Name, converter: NameToFormatStringConverter),
            [!AvaloniaEditableLabel.TextProperty] = CompiledBinding.Create<IInterfaceData<string>, string>(x => x.Value, mode: BindingMode.TwoWay),
            HorizontalAlignment = HorizontalAlignment.Center
        });
    }
}