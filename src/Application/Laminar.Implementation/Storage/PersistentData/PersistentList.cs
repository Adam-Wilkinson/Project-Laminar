using System.Collections;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

internal class PersistentList(IServiceProvider serviceProvider) 
    : PersistentDataNode(serviceProvider), IPersistentList
{
    internal List<IPersistentDataPoint> InternalValues { get; } = [];

    public event EventHandler? ContentsChanged;

    public void Add(IPersistentDataPoint item)
    {
        InternalValues.Add(item);
        OnContentsChanged();
    }

    public void Clear()
    {
        InternalValues.Clear();
        OnContentsChanged();
    }

    public bool Contains(IPersistentDataPoint item) => InternalValues.Contains(item);

    public void CopyTo(IPersistentDataPoint[] array, int arrayIndex) => InternalValues.CopyTo(array, arrayIndex);

    public bool Remove(IPersistentDataPoint item)
    {
        var result = InternalValues.Remove(item);
        OnContentsChanged();
        return result;
    }

    public int Count => InternalValues.Count;
    public bool IsReadOnly => false;

    public IPersistentValue<T> AddAndInitialize<T>(T initialValue, object? deserializationContext = null,
        Type? serializationKeyOverride = null) where T : notnull 
        => InsertAndInitialize(Count, initialValue,  deserializationContext, serializationKeyOverride);

    public IPersistentValue<T> InsertAndInitialize<T>(int index, T initialValue, 
        object? deserializationContext = null, Type? serializationKeyOverride = null) where T : notnull
    {
        var point = CreateValue();
        var result = point.SetDefaultAndGet(initialValue, serializationKeyOverride ?? typeof(T), deserializationContext);
        InternalValues.Insert(index, point);
        OnContentsChanged();
        return result;
    }

    public IPersistentValue<T> GetValue<T>(int index) where T : notnull
        => InternalValues[index].GetValue<T>();

    public void SetValue<T>(int index, T value) where T : notnull
        => InternalValues[index].GetValue<T>().Value = value;
    
    internal void OnContentsChanged() => ContentsChanged?.Invoke(this, EventArgs.Empty);
    
    public IEnumerator<IPersistentDataPoint> GetEnumerator() => InternalValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public int IndexOf(IPersistentDataPoint item) => InternalValues.IndexOf(item);

    public void Insert(int index, IPersistentDataPoint item)
    {
        InternalValues.Insert(index, item);
        OnContentsChanged();
    }

    public void RemoveAt(int index)
    {
        RemoveValue(InternalValues[index]);
        InternalValues.RemoveAt(index);
        OnContentsChanged();
    }

    public IPersistentDataPoint this[int index]
    {
        get => InternalValues[index];
        set
        {
            InternalValues[index] = value;
            OnContentsChanged();
        }
    }
}