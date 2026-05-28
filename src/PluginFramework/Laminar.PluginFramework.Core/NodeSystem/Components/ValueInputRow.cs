using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class ValueInputRow<T> : SingleItemNodeComponent where T : notnull
{
    private readonly IValueInput<T> _valueInput;

    internal ValueInputRow(INodeComponentFactory componentFactory, string name, T? initialValue, Action<T>? valueSetter = null)
    {
        _valueInput = LaminarFactory.NodeIO.ValueInput(name, initialValue: initialValue, setter : valueSetter);
        ChildComponent = componentFactory.CreateSingleRow(_valueInput, _valueInput.InterfaceData, null);
    }

    public T Value => _valueInput.Value;

    public static implicit operator T(ValueInputRow<T> valueInputRow)
    {
        return valueInputRow.Value;
    }

    public IUserInterfaceDefinition? Editor
    {
        get => _valueInput.InterfaceData.Editor;
        set => _valueInput.InterfaceData.Editor = value;
    }

    public IUserInterfaceDefinition? Viewer
    {
        get => _valueInput.InterfaceData.Viewer;
        set => _valueInput.InterfaceData.Viewer = value;
    }
}

public static class ValueInputFactoryExtension
{
    public static ValueInputRow<T> ValueInput<T>(this INodeComponentFactory componentFactory, string name, T? initialValue, 
        IUserInterfaceDefinition? editor = null, IUserInterfaceDefinition? viewer = null, Action<T>? valueAutoSetter = null)
    where T : notnull
        => new(componentFactory, name, initialValue, valueAutoSetter) { Editor = editor, Viewer = viewer };
}