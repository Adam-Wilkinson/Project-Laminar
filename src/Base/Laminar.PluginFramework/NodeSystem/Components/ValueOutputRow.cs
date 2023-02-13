using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class ValueOutputRow<T> : SingleItemNodeComponent
{
    readonly IValueOutput<T> _valueOutput;

    internal ValueOutputRow(INodeComponentFactory factory, string name, T initialValue)
    {
        _valueOutput = LaminarFactory.NodeIO.ValueOutput(name, initialValue);
        ChildComponent = factory.CreateSingleRow(null, _valueOutput.DisplayValue, _valueOutput);
    }

    public void StartValueChangedExecution()
    {
        _valueOutput.StartExecution();
    }

    public T Value
    {
        get => _valueOutput.Value;
        set => _valueOutput.Value = value;
    }

    public IUserInterfaceDefinition? Viewer
    {
        get => _valueOutput.InterfaceDefinition.Viewer;
        set => _valueOutput.InterfaceDefinition.Viewer = value;
    }

    public IUserInterfaceDefinition? Editor
    {
        get => _valueOutput.InterfaceDefinition.Editor;
        set => _valueOutput.InterfaceDefinition.Editor = value;
    }

    public bool IsUserEditable
    {
        get => _valueOutput.InterfaceDefinition.IsUserEditable;
        set => _valueOutput.InterfaceDefinition.IsUserEditable = value;
    }
}

public static class ValueOutputFactoryExtension
{
    public static ValueOutputRow<T> ValueOutput<T>(this INodeComponentFactory componentFactory, string name, T defaultValue, IUserInterfaceDefinition? viewer = null, IUserInterfaceDefinition? editor = null, bool isUserEditable = false) => new(componentFactory, name, defaultValue) { Viewer = viewer, Editor = editor, IsUserEditable = isUserEditable };
}
