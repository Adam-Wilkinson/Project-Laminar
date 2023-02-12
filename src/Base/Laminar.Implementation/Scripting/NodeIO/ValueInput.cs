using System;
using System.Collections.Generic;
using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeIO;

public class ValueInput<T> : IValueInput<T>
{
    readonly LaminarExecutionContext _contextCache;
    readonly IUserInterfaceDefinitionFinder _uiFinder;

    IValueProvider<T>? _valueProvider;
    T _internalValue;

    internal ValueInput(
        IUserInterfaceDefinitionFinder uiFinder,
        ITypeInfoStore typeInfoStore,
        string name,
        T defaultValue)
    {
        _uiFinder = uiFinder;
        Connector = new ValueInputConnector<T>(typeInfoStore) { Input = this };
        Name = name;
        _internalValue = defaultValue;
        _contextCache = new LaminarExecutionContext
        {
            ExecutionFlags = ValueExecutionFlag.Value,
            ExecutionSource = Connector,
        };
    }

    public T Value
    {
        get => _valueProvider is not null ? _valueProvider.Value : _internalValue;
        set
        {
            if (_valueProvider is null && !EqualityComparer<T>.Default.Equals(value, _internalValue))
            {
                _internalValue = value;
                FireValueChange();
            }
        }
    }

    public IInputConnector Connector { get; }

    public virtual Action? PreEvaluateAction => null;

    public ValueInterfaceDefinition ValueUserInterface { get; } = new()
    {
        ValueType = typeof(T),
        IsUserEditable = true,
    };

    public string Name { get; }

    public IUserInterfaceDefinition? InterfaceDefinition => _uiFinder.GetCurrentDefinitionOf(ValueUserInterface);

    public event EventHandler<LaminarExecutionContext>? StartExecution;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetValueProvider(IValueProvider<T>? provider)
    {
        _valueProvider = provider;
        ValueUserInterface.IsUserEditable = _valueProvider is null;
        FireValueChange();
    }

    object? IDisplayValue.Value
    {
        get => Value;
        set
        {
            if (value is T typedValue)
                Value = typedValue;
        }
    }

    public void Refresh() => PropertyChanged?.Invoke(this, IDisplayValue.ValueChangedEventArgs);

    protected void FireValueChange() => StartExecution?.Invoke(this, _contextCache);
}