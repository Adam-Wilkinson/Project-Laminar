using System.Collections.Generic;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentList : PersistentDataNode, IPersistentList
{
    public List<IPersistentDataValue> InternalValues { get; } = [];
    
    public T GetOrCreateChild<T>(int index) where T : notnull
    {
        throw new System.NotImplementedException();
    }

    public IObservableValueWithDefault<T> Initialize<T>(int index, T initialValue, object? deserializationContext = null)
    {
        throw new System.NotImplementedException();
    }

    public IObservableValueWithDefault<T> Get<T>(int index)
    {
        throw new System.NotImplementedException();
    }

    public void Set<T>(int index, T value)
    {
        throw new System.NotImplementedException();
    }

    public void Remove(int index)
    {
        throw new System.NotImplementedException();
    }
}