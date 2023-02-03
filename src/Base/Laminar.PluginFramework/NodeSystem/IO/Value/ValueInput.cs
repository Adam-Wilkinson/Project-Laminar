using System;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem.IO.Value;

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
        if (provider is null or IValueProvider<T>)
        {
            _valueProvider = provider as IValueProvider<T>;
            FireValueChange();
            return true;
        }

        return false;
    }

    protected void FireValueChange() => StartExecution?.Invoke(this, new LaminarExecutionContext
    {
        ExecutionFlags = ValueExecutionFlag.Value,
        ExecutionSource = this,
    });
}