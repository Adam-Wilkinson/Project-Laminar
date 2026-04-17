using System;
using System.Collections.Generic;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataNode(ISerializer serializer, ILogger<PersistentDataValue> valueLogger) : IPersistentDataNode
{
    private readonly Dictionary<string, IPersistentDataValue> _internalValues = [];

    public IPersistentDataTranscoder? Transcoder => Owner?.Transcoder;
    
    public void OnChildChanged() => Owner?.OnChildChanged();

    public IPersistentDataNode GetOrCreateChild(string childName)
    {
        var persistentDataValue = GetPersistentData(childName);
        if (!persistentDataValue.IsInitialized)
        {
            var newResult = new PersistentDataNode(serializer, valueLogger);
            persistentDataValue.Initialize(newResult, typeof(IPersistentDataValue), newResult);
            return newResult;
        }

        return persistentDataValue.Value as IPersistentDataNode
            ?? throw new ArgumentException("Tried to get child data store, but value already exists and is of wrong type", nameof(childName));
    }

    public IPersistentDataValueOwner? Owner { get; set; }

    public IObservableValueWithDefault<T> InitializeDefaultValue<T>(string key, T defaultValue, object? deserializationContext = null,
        Type? serializationKeyOverride = null) where T : notnull
    {
        IPersistentDataValue returnValue = GetPersistentData(key);
        returnValue.Initialize(defaultValue, serializationKeyOverride, deserializationContext);
        return returnValue.Cast<object, T>();
    }

    public IObservableValueWithDefault<T>? TryGetValue<T>(string key) where T : notnull
        => _internalValues.TryGetValue(key, out var result) ? result.Cast<object, T>() : null;
    
    public bool SetValue<T>(string key, T value) where T : notnull
    {
        if (!_internalValues.TryGetValue(key, out var result)) return false;
        
        result.Value = value;
        return true;

    }

    public bool RemoveValue(string key) => _internalValues.Remove(key);
    
    private IPersistentDataValue GetPersistentData(string key)
    {
        if (_internalValues.TryGetValue(key, out IPersistentDataValue? value))
        {
            return value;
        }

        PersistentDataValue newValue = new(serializer, valueLogger)
        {
            Name = key,
            Owner = this
        };

        _internalValues[key] = newValue;
        return newValue;
    }
}