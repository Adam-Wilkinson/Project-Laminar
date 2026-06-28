using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

internal class PersistentDataPoint(IEncodableDataFactory valueFactory) : IPersistentDataPoint
{
    private const string UninitializedValue = "[UNINITIALIZED DATA POINT]";
    
    private IEncodableData? _materializedValue;
    private IPersistentDataTranscoder? _lastTranscoder;
    private object? _encodedValue;
    
    public void Reset()
    {
        _materializedValue?.Invalidated -= ChildInvalidated;
        _materializedValue = null;
        _encodedValue = null;
        Invalidated?.Invoke(this, EventArgs.Empty);
    }

    public T GetOrCreateCollection<T>(T? knownValue) where T : class, IEncodableData
    {
        if (_materializedValue is not null)
        {
            return (T)_materializedValue ?? throw new InvalidOperationException("This persistent data point is of a different type");
        }

        T newCollection = knownValue ?? valueFactory.GetEncodableData<T>();

        if (_lastTranscoder is not null && _encodedValue is not null)
        {
            newCollection.Decode(_lastTranscoder, _encodedValue);
        }
        
        newCollection.Invalidated += ChildInvalidated;
        _materializedValue = newCollection;
        Invalidated?.Invoke(this, EventArgs.Empty);
        return newCollection;
    }

    public IPersistentValue<T> GetValue<T>(Type? serializationKeyOverride = null, object? deserializationContext = null) where T : notnull
    {
        if (_materializedValue is not null)
        {
            return (IPersistentValue<T>)_materializedValue ?? throw new InvalidOperationException("This persistent data point is of a different type");
        }

        if (_lastTranscoder is null || _encodedValue is null)
        {
            throw new InvalidOperationException("In order to get an uninitialized value, this data point needs both a transcoder and encoded value");
        }

        var newValue = valueFactory.GetValueFromEncoded<T>(_encodedValue, _lastTranscoder, serializationKeyOverride,
            deserializationContext);
        newValue.Invalidated += ChildInvalidated;
        _materializedValue = newValue;
        Invalidated?.Invoke(this, EventArgs.Empty);
        return newValue;
    }

    public IPersistentValue<T> GetValueOrInitialize<T>(T defaultValue, Type? serializationKeyOverride = null, 
        object? deserializationContext = null) where T : notnull
    {
        if (_materializedValue is not null)
        {
            return (IPersistentValue<T>)_materializedValue ?? throw new InvalidOperationException("This persistent data point is of a different type");
        }
        
        var newValue = valueFactory.GetValueWithDefault(defaultValue, serializationKeyOverride, deserializationContext);

        if (_encodedValue is not null && _lastTranscoder is not null)
        {
            newValue.Decode(_lastTranscoder, _encodedValue);
        }
        
        newValue.Invalidated += ChildInvalidated;
        _materializedValue = newValue;
        Invalidated?.Invoke(this, EventArgs.Empty);
        return newValue;
    }

    public object Encode(IPersistentDataTranscoder transcoder)
    {
        if (_materializedValue is null)
        {
            _lastTranscoder = transcoder;
            return _encodedValue ?? UninitializedValue;
        }

        if (ReferenceEquals(transcoder, _lastTranscoder) && _encodedValue is not null)
        {
            return _encodedValue;
        }

        _lastTranscoder = transcoder;
        _encodedValue = _materializedValue.Encode(transcoder);
        return _encodedValue;
    }

    public void Decode(IPersistentDataTranscoder transcoder, object encoded)
    {
        _lastTranscoder = transcoder;
        if (ReferenceEquals(_encodedValue, encoded) || Equals(encoded, UninitializedValue)) return;
        
        _encodedValue = encoded;
        _materializedValue?.Decode(transcoder, encoded);
    }

    public event EventHandler? Invalidated;
    
    private void ChildInvalidated(object? sender, EventArgs e)
    {
        _encodedValue = null;
        Invalidated?.Invoke(sender, e);
    }
}