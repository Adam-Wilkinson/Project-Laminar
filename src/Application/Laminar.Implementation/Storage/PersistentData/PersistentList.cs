using System.Collections;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

internal class PersistentList(IEncodableDataFactory dataFactory) : IPersistentList
{
    private readonly List<IPersistentDataPoint> _internalValues = [];

    public IPersistentDataPoint AddNext()
    {
        var newValue = dataFactory.GetDataPoint();
        newValue.OnInvalidated += OnChildInvalidated;
        _internalValues.Add(newValue);
        return newValue;
    }

    public void Clear()
    {
        foreach (var value in _internalValues)
        {
            value.OnInvalidated -= OnChildInvalidated;
        }
        
        _internalValues.Clear();
        OnInvalidated?.Invoke(this, EventArgs.Empty);
    }

    private void OnChildInvalidated(object? sender, EventArgs e)
    {
        OnInvalidated?.Invoke(sender, e);
    }
    
    public bool Remove(IPersistentDataPoint item)
    {
        if (!_internalValues.Remove(item)) return false;
        item.OnInvalidated -= OnChildInvalidated;
        OnInvalidated?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public int Count => _internalValues.Count;
    
    public IEnumerator<IPersistentDataPoint> GetEnumerator() => _internalValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IPersistentDataPoint this[int index] => _internalValues[index];
    
    public object Encode(IPersistentDataTranscoder transcoder) 
        => _internalValues.Select(x => x.Encode(transcoder)).ToArray();

    public void Decode(IPersistentDataTranscoder transcoder, object encoded)
    {
        var decoded = (List<object>)transcoder.DecodeElement(encoded, typeof(List<object>))!;
        for (int i = 0; i < Math.Min(decoded.Count, _internalValues.Count); i++)
        {
            _internalValues[i].Decode(transcoder, decoded[i]);
        }

        if (decoded.Count > _internalValues.Count)
        {
            int currentIndex = _internalValues.Count;
            while (decoded.Count > currentIndex)
            {
                AddNext().Decode(transcoder, decoded[currentIndex]);
                currentIndex++;
            }
        }

        if (_internalValues.Count > decoded.Count)
        {
            while (_internalValues.Count > decoded.Count)
            {
                Remove(_internalValues[^1]);
            }
        }
    }

    public event EventHandler? OnInvalidated;
}