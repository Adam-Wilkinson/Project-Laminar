using Laminar.Contracts.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.PersistentData;

internal class PersistentDataPoint(ISerializer serializer, IServiceProvider serviceProvider) : IPersistentDataPoint
{
    private IEncodablePersistentData? _materializedValue;
    private IPersistentDataTranscoder? _lastTranscoder;
    private object? _encodedValue;
    
    public void Reset()
    {
        _materializedValue?.OnInvalidated -= ChildInvalidated;
        _materializedValue = null;
        _encodedValue = null;
    }

    public T GetOrCreateCollection<T>(T? knownValue) where T : class, IEncodablePersistentData
    {
        if (_materializedValue is not null)
        {
            return (T)_materializedValue ?? throw new InvalidOperationException("This persistent data point is of a different type");
        }

        T newCollection = knownValue ?? serviceProvider.GetRequiredService<T>();

        if (_lastTranscoder is not null && _encodedValue is not null)
        {
            newCollection.Decode(_lastTranscoder, _encodedValue);
        }
        
        newCollection.OnInvalidated += ChildInvalidated;
        _materializedValue = newCollection;
        return newCollection;
    }

    public IPersistentValue<T> GetValue<T>(Type? serializationKeyOverride = null, object? deserializationContext = null) where T : notnull
    {
        if (_materializedValue is IPersistentValue<T> persistentValue)
        {
            return persistentValue;
        }

        if (_lastTranscoder is null || _encodedValue is null)
        {
            throw new InvalidOperationException("In order to get an uninitialized value, this data point needs both a transcoder and encoded value");
        }

        var newValue = PersistentValue<T>.FromEncodedValue(_encodedValue, serializationKeyOverride, 
            deserializationContext, serializer, _lastTranscoder);
        newValue.OnInvalidated += ChildInvalidated;
        _materializedValue = newValue;
        return newValue;
    }

    public IPersistentValue<T> GetValueOrDefault<T>(T defaultValue, Type? serializationKeyOverride = null, 
        object? deserializationContext = null) where T : notnull
    {
        var newValue = new PersistentValue<T>(defaultValue, serializationKeyOverride, deserializationContext, serializer);

        if (_encodedValue is not null && _lastTranscoder is not null)
        {
            newValue.Decode(_lastTranscoder, _encodedValue);
        }
        
        newValue.OnInvalidated += ChildInvalidated;
        _materializedValue = newValue;
        return newValue;
    }

    public object Encode(IPersistentDataTranscoder transcoder)
    {
        if (_materializedValue is null)
        {
            _lastTranscoder = transcoder;
            return _encodedValue ?? throw new InvalidOperationException("Cannot encode uninitialized data point");
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
        if (ReferenceEquals(_encodedValue, encoded)) return;
        
        _encodedValue = encoded;
        _materializedValue?.Decode(transcoder, encoded);
    }

    public event EventHandler? OnInvalidated;
    
    private void ChildInvalidated(object? sender, EventArgs e)
    {
        _encodedValue = null;
        OnInvalidated?.Invoke(sender, e);
    }
}