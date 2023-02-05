using System;
using System.ComponentModel;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeIO;

internal class ValueInput<T> : IValueInput<T>
{
    readonly LaminarExecutionContext _contextCache;
    readonly IUserInterfaceDefinitionFinder _uiFinder;

    IValueProvider<T>? _valueProvider;
    T _internalValue;

    internal ValueInput(
        IUserInterfaceDefinitionFinder uiFinder,
        string name,
        T defaultValue)
    {
        _uiFinder = uiFinder;
        Name = name;
        _internalValue = defaultValue;
        _contextCache = new LaminarExecutionContext
        {
            ExecutionFlags = ValueExecutionFlag.Value,
            ExecutionSource = this,
        };
    }

    public T Value
    {
        get => _valueProvider is not null ? _valueProvider.Value : _internalValue;
        set
        {
            if (_valueProvider is null && !Equals(value, _internalValue))
            {
                _internalValue = value;
                FireValueChange();
            }
        }
    }

    public void SetInternalValue(T newVal)
    {
        _internalValue = newVal;
        FireValueChange();
    }

    public virtual Action? PreEvaluateAction => null;

    public ValueInterfaceDefinition ValueUserInterface { get; } = new()
    {
        ValueType = typeof(T),
        IsUserEditable = true,
    };

    public string Name { get; }

    public IUserInterfaceDefinition InterfaceDefinition => _uiFinder.GetCurrentDefinitionOf(ValueUserInterface);

    public event EventHandler<LaminarExecutionContext>? StartExecution;
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool TrySetValueProvider(object? provider)
    {
        if (provider is null or IValueProvider<T>)
        {
            _valueProvider = provider as IValueProvider<T>;
            FireValueChange();
            return true;
        }

        return false;
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