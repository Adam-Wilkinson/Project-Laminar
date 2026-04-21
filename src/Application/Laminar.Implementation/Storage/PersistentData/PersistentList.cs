using System;
using System.Collections.Generic;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentList(IServiceProvider serviceProvider) : PersistentDataNode(serviceProvider), IPersistentList
{
    internal List<IPersistentDataValue> InternalValues { get; } = [];

    public int Count => InternalValues.Count;
    
    public IObservableValueWithDefault<T> AddAndInitialize<T>(T initialValue, object? deserializationContext = null,
        Type? serializationKeyOverride = null) where T : notnull 
        => InsertAndInitialize(Count, initialValue,  deserializationContext, serializationKeyOverride);

    public IObservableValueWithDefault<T> InsertAndInitialize<T>(int index, T initialValue, 
        object? deserializationContext = null, Type? serializationKeyOverride = null) where T : notnull
    {
        IPersistentDataValue value = CreateValue();
        value.Initialize(initialValue, serializationKeyOverride ?? typeof(T), deserializationContext);
        InternalValues.Insert(index, value);
        return value.Cast<object, T>();
    }

    public IObservableValueWithDefault<T> GetValue<T>(int index)
        => InternalValues[index].Cast<object, T>();

    public void SetValue<T>(int index, T value) where T : notnull
        => InternalValues[index].Value = value;

    public void RemoveValue(int index)
    {
        RemoveValue(InternalValues[index]);
        InternalValues.RemoveAt(index);
    }
}