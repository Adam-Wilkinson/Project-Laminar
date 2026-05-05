using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.UserInterface;

public class SourcedDataInterface<T>(IUserInterfaceDefinition? editor, IUserInterfaceDefinition? viewer) 
    : ISourcedInterfaceData<T> where T : notnull
{
    private readonly PropertyChangedEventArgs _valueChangedEventArgs = new(nameof(Value));
    private readonly PropertyChangedEventArgs _nameChangedEventArgs = new(nameof(Name));

    private T? _valueAtLastRefresh;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public required string Name
    {
        get;
        set => SetField(ref field, value, _nameChangedEventArgs);
    }

    public required T Value
    {
        get => ValueProvider is null ? field : ValueProvider.Value;
        set
        {
            if (!IsUserEditable || !SetField(ref field, value)) return;
            ExecutionStarted?.Invoke(this, new LaminarExecutionContext(this, ExecutionFlags.ValueChanged));
        }
    }

    public bool IsUserEditable
    {
        get => field && ValueProvider is null;
        set
        {
            // We have a value provider, so update silently
            if (ValueProvider is not null)
            {
                field = value;
                return;
            }

            if (!SetField(ref field, value)) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Definition)));
        }
    } = true;

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