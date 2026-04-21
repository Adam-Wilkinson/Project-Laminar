using System;
using System.Collections.Generic;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDictionary(IServiceProvider serviceProvider) 
    : PersistentDataNode(serviceProvider), IPersistentDictionary
{
    internal Dictionary<string, IPersistentDataValue> InternalValues { get; } = [];

    public IObservableValueWithDefault<T> InitializeValue<T>(string key, T defaultValue, 
        object? deserializationContext = null, Type? serializationKeyOverride = null) where T : notnull
    {
        IPersistentDataValue returnValue = GetPersistentData(key);
        returnValue.Initialize(defaultValue, serializationKeyOverride ?? typeof(T), deserializationContext);
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

    public bool RemoveValue(string key)
    {
        RemoveValue(InternalValues[key]);
        return InternalValues.Remove(key);
    }
    
    public IPersistentDataValue GetPersistentData(string key)
    {
        if (InternalValues.TryGetValue(key, out IPersistentDataValue? value))
        {
            return value;
        }

        var newValue = CreateValue();
        InternalValues[key] = newValue;
        return newValue;
    }
}