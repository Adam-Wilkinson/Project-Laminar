using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class ValueInputRow<T> : SingleItemNodeComponent
{
    readonly IValueInput<T> _valueInput;

    internal ValueInputRow(INodeComponentFactory componentFactory, string name, T initialValue)
    {
        _valueInput = LaminarFactory.NodeIO.ValueInput(name, initialValue);
        ChildComponent = componentFactory.Row(_valueInput, _valueInput, null);
    }

    public T Value => _valueInput.Value;

    public static implicit operator T(ValueInputRow<T> valueInputRow)
    {
        return valueInputRow.Value;
    }

    public IUserInterfaceDefinition? Editor
    {
        get => _valueInput.ValueUserInterface.Editor;
        set => _valueInput.ValueUserInterface.Editor = value;
    }

    public IUserInterfaceDefinition? Viewer
    {
        get => _valueInput.ValueUserInterface.Viewer;
        set => _valueInput.ValueUserInterface.Viewer = value;
    }
}

public static class ValueInputFactoryExtension
{
    public static ValueInputRow<T> ValueInput<T>(this INodeComponentFactory componentFactory, string name, T defaultValue, IUserInterfaceDefinition? editor = null, IUserInterfaceDefinition? viewer = null) => new(componentFactory, name, defaultValue) { Editor = editor, Viewer = viewer };
}