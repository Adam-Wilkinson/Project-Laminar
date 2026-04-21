using System;
using System.Collections.Generic;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDictionary(IServiceProvider serviceProvider) 
    : PersistentDataNode(serviceProvider), IPersistentDictionary
{
    internal Dictionary<string, IPersistentDataPoint> InternalValues { get; } = [];

    public IPersistentValue<T> InitializeValue<T>(string key, T defaultValue, 
        object? deserializationContext = null, Type? serializationKeyOverride = null) where T : notnull 
        => GetPersistentData(key).Initialize(defaultValue, serializationKeyOverride ?? typeof(T), deserializationContext);

    public IPersistentValue<T>? TryGetValue<T>(string key) where T : notnull
        => InternalValues.TryGetValue(key, out var result) ? result.GetValue<T>() : null;
    
    public bool SetValue<T>(string key, T value) where T : notnull
    {
        if (!InternalValues.TryGetValue(key, out var result)) return false;
        
        result.GetValue<T>().Value = value;
        return true;

    }

    public bool RemoveValue(string key)
    {
        RemoveValue(InternalValues[key]);
        return InternalValues.Remove(key);
    }
    
    public IPersistentDataPoint GetPersistentData(string key)
    {
        if (InternalValues.TryGetValue(key, out IPersistentDataPoint? value))
        {
            return value;
        }

        var newValue = CreateValue();
        InternalValues[key] = newValue;
        return newValue;
    }
}