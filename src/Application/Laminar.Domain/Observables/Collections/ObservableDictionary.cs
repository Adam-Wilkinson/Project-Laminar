using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Laminar.Domain.Observables.Collections;

public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey: notnull
{
    private readonly Dictionary<TKey, TValue> _internal = new();
    
    public IReadOnlyObservableBag<TKey> Keys => field ??= new ManualObservableBag<TKey>(_internal.Keys);
    
    public IReadOnlyObservableBag<TValue> Values => field ??= new ManualObservableBag<TValue>(_internal.Values);
    
    private ManualObservableBag<TKey> NotifyingKeys => (ManualObservableBag<TKey>)Keys;
    
    private ManualObservableBag<TValue> NotifyingValues => (ManualObservableBag<TValue>)Values;
    
    public void Clear()
    {
        var cache = _internal.ToList();
        _internal.Clear();
        foreach (var (key, value) in cache)
        {
            NotifyingKeys.NotifyItemRemoved(key);
            NotifyingValues.NotifyItemRemoved(value);
        }
    }

    public int Count => _internal.Count;
    public void Add(TKey key, TValue value)
    {
        _internal.Add(key, value);
        NotifyingKeys.NotifyItemAdded(key);
        NotifyingValues.NotifyItemAdded(value);
    }

    public bool ContainsKey(TKey key) => _internal.ContainsKey(key);

    public bool Remove(TKey key)
    {
        if (!_internal.Remove(key, out var removedValue))
        {
            return false;
        }
        
        NotifyingKeys.NotifyItemRemoved(key);
        NotifyingValues.NotifyItemRemoved(removedValue);
        return true;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _internal.TryGetValue(key, out value);

    public TValue this[TKey key]
    {
        get => _internal[key];
        set
        {
            var existing = _internal.GetValueOrDefault(key);
            _internal[key] = value;
            if (existing is not null)
            {
                NotifyingValues.NotifyItemRemoved(existing);
            }
            NotifyingValues.NotifyItemAdded(value);
        }
    }
    
    public bool IsReadOnly => false;
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _internal.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _internal.ToList().CopyTo(array, arrayIndex);
    public bool Remove(KeyValuePair<TKey, TValue> item) => _internal.Remove(item.Key);
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => _internal.Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => _internal.Values;
    
    private class ManualObservableBag<T>(ICollection<T> collection) : IReadOnlyObservableBag<T>
    {
        public int Count => collection.Count;
        
        public event EventHandler<T>? ItemAdded;
        public event EventHandler<T>? ItemRemoved;
        
        public IEnumerator<T> GetEnumerator() => collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Contains(T value) => collection.Contains(value);
        
        public void NotifyItemAdded(T item) => ItemAdded?.Invoke(this, item);
        
        public void NotifyItemRemoved(T item) => ItemRemoved?.Invoke(this, item);
    }
}