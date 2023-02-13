using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class ValueOutputRow<T> : SingleItemNodeComponent
{
    readonly IValueOutput<T> _valueOutput;

    internal ValueOutputRow(INodeComponentFactory factory, string name, T initialValue)
    {
        _valueOutput = LaminarFactory.NodeIO.ValueOutput(name, initialValue);
        ChildComponent = factory.CreateSingleRow(null, _valueOutput, _valueOutput);
    }

    public T Value
    {
        get => _valueOutput.Value;
        set => _valueOutput.Value = value;
    }

    public IUserInterfaceDefinition? Viewer
    {
        get => _valueOutput.ValueUserInterface.Viewer;
        set => _valueOutput.ValueUserInterface.Viewer = value;
    }

    public IUserInterfaceDefinition? Editor
    {
        get => _valueOutput.ValueUserInterface.Editor;
        set => _valueOutput.ValueUserInterface.Editor = value;
    }

    public bool IsUserEditable
    {
        get => _valueOutput.ValueUserInterface.IsUserEditable;
        set => _valueOutput.ValueUserInterface.IsUserEditable = value;
    }
}

public static class ValueOutputFactoryExtension
{
    public static ValueOutputRow<T> ValueOutput<T>(this INodeComponentFactory componentFactory, string name, T defaultValue, IUserInterfaceDefinition? viewer = null, IUserInterfaceDefinition? editor = null, bool isUserEditable = false) => new(componentFactory, name, defaultValue) { Viewer = viewer, Editor = editor, IsUserEditable = isUserEditable };
}
