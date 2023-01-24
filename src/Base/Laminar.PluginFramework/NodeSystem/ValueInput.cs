using System;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.PluginFramework.NodeSystem;

public class ValueInput<T> : IValueInput
{
    IValueProvider<T>? _valueProvider;
    protected T _internalValue;

    public ValueInput(string name, T defaultValue)
    {
        Name = name;
        _internalValue = defaultValue;
    }

    public string Name { get; }

    public bool IsUserEditable => _valueProvider is null;

    public IUserInterfaceDefinition? Editor { get; set; }

    public IUserInterfaceDefinition? Viewer { get; set; }

    public T Value => _valueProvider is not null ? _valueProvider.Value : _internalValue;

    public void SetInternalValue(T newVal)
    {
        _internalValue = newVal;
        FireValueChange();
    }

    object? IValueInfo.BoxedValue
    {
        get => Value;
        set
        {
            if (value is T typedValue && _valueProvider is null)
            {
                _internalValue = typedValue;
                FireValueChange();
            }
        }
    }

    Type? IValueInfo.ValueType => typeof(T);

    public virtual Action? PreEvaluateAction => null;

    public event EventHandler<LaminarExecutionContext>? StartExecution;

    public bool TrySetValueProvider(object? provider)
    {
        if (provider is null)
        {
            _valueProvider = null;
            FireValueChange();
            return true;
        }

        if (provider is IValueProvider<T> valueProvider)
        {
            _valueProvider = valueProvider;
            FireValueChange();
            return true;
        }

        return false;
    }

    public static implicit operator T(ValueInput<T> inputValue) => inputValue.Value;

    protected void FireValueChange() => StartExecution?.Invoke(this, new LaminarExecutionContext
    {
        ExecutionFlags = ValueExecutionFlag.Value,
        ExecutionSource = this,
    });
}