using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class ValueInputRow<T> : SingleItemNodeComponent
{
    readonly IValueInput<T> _valueInput;

    internal ValueInputRow(INodeComponentFactory componentFactory, string name, T initialValue)
    {
        _valueInput = LaminarFactory.NodeIO.ValueInput(name, initialValue);
        ChildComponent = componentFactory.CreateSingleRow(_valueInput, _valueInput.DisplayValue, null);
    }

    public T Value => _valueInput.Value;

    public static implicit operator T(ValueInputRow<T> valueInputRow)
    {
        return valueInputRow.Value;
    }

    public IUserInterfaceDefinition? Editor
    {
        get => _valueInput.InterfaceDefinition.Editor;
        set => _valueInput.InterfaceDefinition.Editor = value;
    }

    public IUserInterfaceDefinition? Viewer
    {
        get => _valueInput.InterfaceDefinition.Viewer;
        set => _valueInput.InterfaceDefinition.Viewer = value;
    }
}

public static class ValueInputFactoryExtension
{
    public static ValueInputRow<T> ValueInput<T>(this INodeComponentFactory componentFactory, string name, T defaultValue, IUserInterfaceDefinition? editor = null, IUserInterfaceDefinition? viewer = null) => new(componentFactory, name, defaultValue) { Editor = editor, Viewer = viewer };
}