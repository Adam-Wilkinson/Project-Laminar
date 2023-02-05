using System;
using System.ComponentModel;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeIO;

public class ValueOutput<T> : IValueOutput<T>
{
    private readonly IUserInterfaceDefinitionFinder _uiFinder;

    public ValueOutput(
        IUserInterfaceDefinitionFinder uiFinder,
        string name,
        T initialValue)
    {
        Name = name;
        _uiFinder = uiFinder;
        Value = initialValue;
    }

    public virtual T Value { get; set; }

    public Action? PreEvaluateAction => null;

    public ValueInterfaceDefinition ValueUserInterface { get; } = new() { IsUserEditable = false, ValueType = typeof(T) };

    public string Name { get; }

    public IUserInterfaceDefinition InterfaceDefinition => _uiFinder.GetCurrentDefinitionOf(ValueUserInterface);

    object? IDisplayValue.Value
    {
        get => Value;
        set { }
    }

    public event EventHandler<LaminarExecutionContext>? StartExecution;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Refresh() => PropertyChanged?.Invoke(this, IDisplayValue.ValueChangedEventArgs);
}