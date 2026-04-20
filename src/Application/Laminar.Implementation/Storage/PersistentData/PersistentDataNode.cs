using System;
using System.Collections.Generic;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataNode(ISerializer serializer, IExceptionHandler exceptionHandler, ILogger<PersistentDataValue> valueLogger) : IPersistentDataNode
{
    public Dictionary<string, IPersistentDataValue> InternalValues { get; } = [];

    public IPersistentDataTranscoder? Transcoder => Owner?.Transcoder;

    public event EventHandler? ChildValueChanged;
    
    public event EventHandler? TranscoderChanged;

    public void OnChildValueChanged()
    {
        ChildValueChanged?.Invoke(this, EventArgs.Empty);
        Owner?.OnChildValueChanged();
    }

    public IPersistentDataNode GetOrCreateChild(string childName)
    {
        var persistentDataValue = GetPersistentData(childName);
        if (!persistentDataValue.IsInitialized)
        {
            var newResult = new PersistentDataNode(serializer, exceptionHandler, valueLogger);
            persistentDataValue.Initialize(newResult, typeof(PersistentDataNode), newResult);
            return newResult;
        }

        return persistentDataValue.Value as IPersistentDataNode
            ?? throw new ArgumentException("Tried to get child data store, but value already exists and is of wrong type", nameof(childName));
    }

    public IPersistentDataValueOwner? Owner
    {
        get;
        set
        {
            if (field is not null)
            {
                field.TranscoderChanged -= OnOwnerTranscoderChanged;
            }
            
            field = value;

            if (field is not null)
            {
                field.TranscoderChanged += OnOwnerTranscoderChanged;
            }
            
            TranscoderChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public IObservableValueWithDefault<T> InitializeDefaultValue<T>(string key, T defaultValue, object? deserializationContext = null,
        Type? serializationKeyOverride = null) where T : notnull
    {
        IPersistentDataValue returnValue = GetPersistentData(key);
        returnValue.Initialize(defaultValue, serializationKeyOverride, deserializationContext);
        return returnValue.Cast<object, T>();
    }

    public IObservableValueWithDefault<T>? TryGetValue<T>(string key) where T : notnull
        => InternalValues.TryGetValue(key, out var result) ? result.Cast<object, T>() : null;
    
    public bool SetValue<T>(string key, T value) where T : notnull
    {
        if (!InternalValues.TryGetValue(key, out var result)) return false;
        
        result.Value = value;
        return true;

    }

    public bool RemoveValue(string key) => InternalValues.Remove(key);
    
    public IPersistentDataValue GetPersistentData(string key)
    {
        if (InternalValues.TryGetValue(key, out IPersistentDataValue? value))
        {
            return value;
        }

        PersistentDataValue newValue = new(this, serializer, exceptionHandler, valueLogger)
        {
            Name = key,
        };

        InternalValues[key] = newValue;
        return newValue;
    }
    
    private void OnOwnerTranscoderChanged(object? sender, EventArgs e)
    {
        TranscoderChanged?.Invoke(this, EventArgs.Empty);
    }
}