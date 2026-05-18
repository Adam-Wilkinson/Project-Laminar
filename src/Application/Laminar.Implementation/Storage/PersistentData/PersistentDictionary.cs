using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

internal class PersistentDictionary(IServiceProvider serviceProvider) 
    : PersistentDataNode(serviceProvider), IPersistentDictionary
{
    internal Dictionary<string, IPersistentDataPoint> InternalValues { get; } = [];

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

    public IEnumerator<KeyValuePair<string, IPersistentDataPoint>> GetEnumerator() => InternalValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(KeyValuePair<string, IPersistentDataPoint> item) => InternalValues.Add(item.Key, item.Value);

    public void Clear() => InternalValues.Clear();

    public bool Contains(KeyValuePair<string, IPersistentDataPoint> item) => InternalValues.ContainsKey(item.Key);

    public void CopyTo(KeyValuePair<string, IPersistentDataPoint>[] array, int arrayIndex) 
        => ((ICollection<KeyValuePair<string, IPersistentDataPoint>>)InternalValues).CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<string, IPersistentDataPoint> item) => InternalValues.Remove(item.Key);

    public int Count => InternalValues.Count;
    public bool IsReadOnly => false;

    public void Add(string key, IPersistentDataPoint value) => InternalValues.Add(key, value);

    public bool ContainsKey(string key) => InternalValues.ContainsKey(key);

    public bool Remove(string key) => InternalValues.Remove(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IPersistentDataPoint value)
        => InternalValues.TryGetValue(key, out value);

    public IPersistentDataPoint this[string key]
    {
        get => GetPersistentData(key);
        set => InternalValues[key] = value;
    }

    public ICollection<string> Keys => InternalValues.Keys;
    public ICollection<IPersistentDataPoint> Values => InternalValues.Values;
}