using System;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.PluginFramework.NodeSystem;

public class ValueInput<T> : IValueInput
{
    IValueProvider<T>? _valueProvider;
    T _internalValue;

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

    private void FireValueChange()
    {
        StartExecution?.Invoke(this, new LaminarExecutionContext(null, ExecutionFlags.ValuesChanged, DateTime.Now));
        //{
        //    ExecutionSource = null,
        //    ExecutionFlags = ExecutionFlags.ValuesChanged,
        //    TimeOfStart = DateTime.Now,
        //});
    }
}