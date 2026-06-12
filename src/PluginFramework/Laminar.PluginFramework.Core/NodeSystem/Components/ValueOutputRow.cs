using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class ValueOutputRow<T> : SingleItemNodeComponent where T : notnull
{
    private readonly IValueOutput<T> _valueOutput;

    internal ValueOutputRow(INodeComponentFactory factory, string name, T? initialValue, Func<T>? manualValueGetter)
    {
        _valueOutput = LaminarFactory.NodeIO.ValueOutput(name, initialValue: initialValue, getter: manualValueGetter);
        ChildComponent = factory.CreateSingleRow(null, _valueOutput.InterfaceData, _valueOutput);
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
        get => _valueOutput.InterfaceData.Viewer;
        set => _valueOutput.InterfaceData.Viewer = value;
    }

    public IUserInterfaceDefinition? Editor
    {
        get => _valueOutput.InterfaceData.Editor;
        set => _valueOutput.InterfaceData.Editor = value;
    }

    public bool IsUserEditable
    {
        get => _valueOutput.InterfaceData.IsUserEditable;
        set => _valueOutput.InterfaceData.IsUserEditable = value;
    }
}

public static class ValueOutputFactoryExtension
{
    public static ValueOutputRow<T> ValueOutput<T>(this INodeComponentFactory componentFactory, string name,
        T? initialValue = default, IUserInterfaceDefinition? viewer = null, IUserInterfaceDefinition? editor = null,
        bool isUserEditable = false, Func<T>? manualValueGetter = null) where T : notnull
        => new(componentFactory, name, initialValue, manualValueGetter)
        {
            Viewer = viewer, 
            Editor = editor, 
            IsUserEditable = isUserEditable
        };
}
