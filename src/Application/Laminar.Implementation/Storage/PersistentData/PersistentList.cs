using System;
using System.Collections.Generic;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentList(IServiceProvider serviceProvider) : PersistentDataNode(serviceProvider), IPersistentList
{
    internal List<IPersistentDataPoint> InternalValues { get; } = [];

    public int Count => InternalValues.Count;
    
    public IPersistentValue<T> AddAndInitialize<T>(T initialValue, object? deserializationContext = null,
        Type? serializationKeyOverride = null) where T : notnull 
        => InsertAndInitialize(Count, initialValue,  deserializationContext, serializationKeyOverride);

    public IPersistentValue<T> InsertAndInitialize<T>(int index, T initialValue, 
        object? deserializationContext = null, Type? serializationKeyOverride = null) where T : notnull
    {
        var point = CreateValue();
        var result = point.Initialize(initialValue, serializationKeyOverride ?? typeof(T), deserializationContext);
        InternalValues.Insert(index, point);
        return result;
    }

    public IPersistentValue<T> GetValue<T>(int index)
        => InternalValues[index].GetValue<T>();

    public void SetValue<T>(int index, T value) where T : notnull
        => InternalValues[index].GetValue<T>().Value = value;

    public void RemoveValue(int index)
    {
        RemoveValue(InternalValues[index]);
        InternalValues.RemoveAt(index);
    }
}