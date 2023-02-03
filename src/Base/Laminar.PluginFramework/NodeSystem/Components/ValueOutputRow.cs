using System;
using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class ValueOutputRow<T> : SingleItemNodeComponent
{
    readonly ValueOutput<T> _valueOutput;

    public ValueOutputRow(string name, T defaultValue)
    {
        _valueOutput = new(name, defaultValue);
        ChildComponent = Component.Row(null, _valueOutput, _valueOutput);
    }

    public T Value
    {
        get => _valueOutput.Value;
        set => _valueOutput.Value = value;
    }
}
