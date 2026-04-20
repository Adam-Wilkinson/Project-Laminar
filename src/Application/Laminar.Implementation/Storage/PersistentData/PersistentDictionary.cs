using System;
using System.Collections.Generic;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDictionary(IServiceProvider serviceProvider) 
    : PersistentDataNode, IPersistentDictionary
{
    public Dictionary<string, IPersistentDataValue> InternalValues { get; } = [];

    public T GetOrCreateChild<T>(string childName) where T : class, IPersistentDataValueOwner
    {
        var persistentDataValue = GetPersistentData(childName);
        if (!persistentDataValue.IsInitialized)
        {
            var newResult = serviceProvider.GetRequiredService<T>();
            persistentDataValue.Initialize(newResult, typeof(T));
            return newResult;
        }
        
        return persistentDataValue.Value as T 
               ?? throw new ArgumentException("Tried to get child data store, but value already exists and is of wrong type", nameof(childName));
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

        var newValue = ActivatorUtilities.CreateInstance<PersistentDataValue>(serviceProvider, this, key);
        InternalValues[key] = newValue;
        return newValue;
    }
}