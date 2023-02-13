using System;
using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeIO;

public class ValueOutput<T> : IValueOutput<T>
{
    readonly LaminarExecutionContext _contextCache;
    private readonly IUserInterfaceDefinitionFinder _uiFinder;

    public ValueOutput(
        IUserInterfaceDefinitionFinder uiFinder,
        ITypeInfoStore typeInfoStore,
        string name,
        T initialValue)
    {
        Connector = new ValueOutputConnector<T>(typeInfoStore, this);
        Name = name;
        _uiFinder = uiFinder;
        Value = initialValue;
        _contextCache = new LaminarExecutionContext
        {
            ExecutionFlags = ValueExecutionFlag.Value,
            ExecutionSource = Connector,
        };
    }

    public T Value { get; set; }

    public Action? PreEvaluateAction => null;

    public ValueInterfaceDefinition ValueUserInterface { get; } = new() { IsUserEditable = false, ValueType = typeof(T) };

    public string Name { get; }

    public IUserInterfaceDefinition? InterfaceDefinition => _uiFinder.GetCurrentDefinitionOf(ValueUserInterface);

    public IOutputConnector Connector { get; }

    public bool AlwaysPassUpdate { get; init; }

    object? IDisplayValue.Value
    {
        get => Value;
        set 
        {
            if (value is T typedValue)
            {
                Value = typedValue;
                FireValueChange();
            }
        }
    }

    public event EventHandler<LaminarExecutionContext>? StartExecution;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Refresh() => PropertyChanged?.Invoke(this, IDisplayValue.ValueChangedEventArgs);

    protected void FireValueChange() => StartExecution?.Invoke(this, _contextCache);
}