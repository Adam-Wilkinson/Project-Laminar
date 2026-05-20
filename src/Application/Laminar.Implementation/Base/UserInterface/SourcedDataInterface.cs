using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.UserInterface;

public class SourcedDataInterface<T>(T initialValue, IUserInterfaceDefinition? editor, IUserInterfaceDefinition? viewer) 
    : ISourcedInterfaceData<T> where T : notnull
{
    private readonly PropertyChangedEventArgs _valueChangedEventArgs = new(nameof(Value));
    private readonly PropertyChangedEventArgs _nameChangedEventArgs = new(nameof(Name));

    private bool _isUserEditablePreference = true;
    private T? _valueAtLastRefresh;
    private T _value = initialValue;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public required string Name
    {
        get;
        set => SetField(ref field, value, _nameChangedEventArgs);
    }

    public T Value
    {
        get => ValueProvider is null ? _value : ValueProvider.Value;
        set
        {
            if (!IsUserEditable || !SetField(ref _value, value, _valueChangedEventArgs)) return;
            ExecutionStarted?.Invoke(this, new LaminarExecutionContext(null, ExecutionFlags.ValueChanged));
        }
    }

    public void QuietSetValue(T value)
    {
        SetField(ref _value, value, _valueChangedEventArgs);
    }

    public bool IsUserEditable
    {
        get => _isUserEditablePreference && ValueProvider is null;
        set
        {
            // We have a value provider, so update silently
            if (ValueProvider is not null)
            {
                _isUserEditablePreference = value;
                return;
            }

            if (!SetField(ref _isUserEditablePreference, value)) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Definition)));
        }
    }

    public IUserInterfaceDefinition? Definition => IsUserEditable ? editor : viewer;

    public IValueProvider<T>? ValueProvider
    {
        get;
        set
        {
            if (!SetField(ref field, value)) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUserEditable)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Definition)));
            Refresh();
            ExecutionStarted?.Invoke(this, new LaminarExecutionContext(null, ExecutionFlags.ValueChanged));
        }
    }

    public void Refresh()
    {
        if (EqualityComparer<T>.Default.Equals(Value, _valueAtLastRefresh)) return;
        _valueAtLastRefresh = Value;
        PropertyChanged?.Invoke(this, _valueChangedEventArgs);
    }

    private bool SetField<TField>(ref TField field, TField value, [CallerMemberName] string? propertyName = null)
        => SetField(ref field, value, new PropertyChangedEventArgs(propertyName));
    
    private bool SetField<TField>(ref TField field, TField value, PropertyChangedEventArgs args)
    {
        if (EqualityComparer<TField>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, args);
        return true;
    }

    public event EventHandler<LaminarExecutionContext>? ExecutionStarted;
}