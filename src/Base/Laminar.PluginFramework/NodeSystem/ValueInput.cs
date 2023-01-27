using System;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.PluginFramework.NodeSystem;

public class ValueInput<T> : IValueInput, IConvertsToNodeComponent
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

    public static implicit operator T(ValueInput<T> inputValue) => inputValue.Value;

    protected void FireValueChange() => StartExecution?.Invoke(this, new LaminarExecutionContext
    {
        ExecutionFlags = ValueExecutionFlag.Value,
        ExecutionSource = this,
    });

    public NodeComponent GetComponent() => NodeComponent.Row(this, this, null);
}