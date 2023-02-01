using System;
using System.Collections;
using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.Components;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem;

public class ValueOutput<T> : IValueOutput, IValueProvider<T>, INodeComponent
{
    INodeRow _asRow;

    public ValueOutput(string name, T initialValue)
    {
        Name = name;
        Value = initialValue;
        _asRow = Component.Row(null, this, this);
        _asRow.Opacity.AddFactor(Opacity);
    }

    public virtual T Value { get; set; }

    public string Name { get; }

    public IUserInterfaceDefinition? Editor { get; set; }

    public IUserInterfaceDefinition? Viewer { get; set; }

    public bool IsUserEditable => false;

    public Action? PreEvaluateAction => null;

    public Opacity Opacity { get; } = new();

    Type? IValueInfo.ValueType => typeof(T);

    object? IValueInfo.BoxedValue 
    { 
        get => Value; 
        set 
        { 
            if (value is T typedValue) 
            { 
                Value = typedValue; 
            } 
        } 
    }

    public event EventHandler<LaminarExecutionContext>? StartExecution;

    public IEnumerator<INodeComponent> GetEnumerator()
    {
        yield return _asRow;
    }

    protected void FireExecutionEvent() => StartExecution?.Invoke(this, new LaminarExecutionContext
    {
        ExecutionFlags = ValueExecutionFlag.Value,
        ExecutionSource = this,
    });

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}