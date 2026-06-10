using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.PersistentData;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.PersistentData;

internal class PersistentDictionary(IServiceProvider serviceProvider) : IPersistentDictionary
{
    private readonly Dictionary<string, IPersistentDataPoint> _internalValues = [];
    
    public object Encode(IPersistentDataTranscoder transcoder) =>
        transcoder.EncodeElement(_internalValues.ToDictionary(
            x => x.Key, 
            x => x.Value.Encode(transcoder)))
        ?? throw new InvalidOperationException();

    public void Decode(IPersistentDataTranscoder transcoder, object encoded)
    {
        var dictionary = (Dictionary<string, object>)transcoder.DecodeElement(encoded, typeof(Dictionary<string, object>))!;
        foreach (var (key, value) in dictionary)
        {
            GetPersistentData(key).Decode(transcoder, value);
        }
    }

    public bool Remove(string key)
    {
        if (!_internalValues.Remove(key, out var value)) return false;
        value.OnInvalidated -= ChildInvalidated;
        OnInvalidated?.Invoke(this, EventArgs.Empty);
        return true;
    }
    
    public void Clear()
    {
        foreach (var value in _internalValues.Values)
        {
            value.OnInvalidated -= ChildInvalidated;
        }
        _internalValues.Clear();
        OnInvalidated?.Invoke(this, EventArgs.Empty);
    }

    private void ChildInvalidated(object? sender, EventArgs e)
    {
        OnInvalidated?.Invoke(sender, e);
    }

    public IPersistentDataPoint GetPersistentData(string key)
    {
        if (_internalValues.TryGetValue(key, out IPersistentDataPoint? value))
        {
            return value;
        }

        var newValue = ActivatorUtilities.CreateInstance<PersistentDataPoint>(serviceProvider);
        _internalValues[key] = newValue;
        newValue.OnInvalidated += ChildInvalidated;
        OnInvalidated?.Invoke(this, EventArgs.Empty);
        return newValue;
    }

    public IEnumerator<KeyValuePair<string, IPersistentDataPoint>> GetEnumerator() => _internalValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public int Count => _internalValues.Count;
    
    public bool ContainsKey(string key) => _internalValues.ContainsKey(key);
    
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IPersistentDataPoint value)
        => _internalValues.TryGetValue(key, out value);

    public IPersistentDataPoint this[string key] => GetPersistentData(key);
    
    public IEnumerable<string> Keys => _internalValues.Keys;
    
    public IEnumerable<IPersistentDataPoint> Values => _internalValues.Values;

    public event EventHandler? OnInvalidated;
}