using System;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.PluginFramework.NodeSystem;

public class ValueOutput<T> : IValueOutput, IValueProvider<T>
{
    public ValueOutput(string name, T initialValue)
    {
        Name = name;
        Value = initialValue;
    }

    public virtual T Value { get; set; }

    public string Name { get; }

    public IUserInterfaceDefinition? Editor { get; set; }

    public IUserInterfaceDefinition? Viewer { get; set; }

    public bool IsUserEditable => false;

    public Action? PreEvaluateAction => null;

    Type? IValueInfo.ValueType => typeof(T);

    object? IValueInfo.BoxedValue { get => Value; set { if (value is T typedValue) Value = typedValue; } }

    public event EventHandler<LaminarExecutionContext>? StartExecution;
}