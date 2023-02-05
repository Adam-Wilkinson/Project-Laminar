using System;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class ValueOutputRow<T> : SingleItemNodeComponent
{
    readonly IValueOutput<T> _valueOutput;

    public ValueOutputRow(string name, T defaultValue)
    {
        _valueOutput =  NodeIO.ValueOutput(name, defaultValue);
        ChildComponent = Component.Row(null, _valueOutput, _valueOutput);
    }

    public T Value
    {
        get => _valueOutput.Value;
        set => _valueOutput.Value = value;
    }

    public IUserInterfaceDefinition? Editor
    {
        get => _valueOutput.ValueUserInterface.Editor;
        set => _valueOutput.ValueUserInterface.Editor = value;
    }

    public IUserInterfaceDefinition? Viewer
    {
        get => _valueOutput.ValueUserInterface.Viewer;
        set => _valueOutput.ValueUserInterface.Viewer = value;
    }
}
