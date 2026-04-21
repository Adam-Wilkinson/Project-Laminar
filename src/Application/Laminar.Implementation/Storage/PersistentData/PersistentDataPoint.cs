using System;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataPoint : IPersistentDataPoint
{
    private readonly ISerializer _serializer;
    private readonly ILogger<PersistentDataPoint> _logger;
    private readonly IExceptionHandler _exceptionHandler;

    private IPersistentValueInternal? _persistentValue;
    private object? _encodedValue;

    public PersistentDataPoint(IPersistentDataValueOwner owner,
        ISerializer serializer, 
        IExceptionHandler exceptionHandler,
        ILogger<PersistentDataPoint> logger)
    {
        _serializer = serializer;
        _logger = logger;
        Owner = owner;
        _exceptionHandler = exceptionHandler;
        Owner.TranscoderChanged += (_, _) =>
        {
            if (IsInitialized) UpdateEncodedFromValue();
        };
    }

    public IPersistentDataValueOwner Owner { get; }
    
    public bool IsInitialized => _persistentValue is not null;

    public object EncodedValue
    {
        get => _encodedValue ?? throw new ValueNotInitializedException();
        set
        {
            if (Equals(value, _encodedValue)) return;
            _encodedValue = value;

            if (!IsInitialized) return;
            
            if (!UpdateValueFromEncoded())
            {
                _exceptionHandler.OnException(new ErrorDecodingValueException());
                UpdateEncodedFromValue();
            }
        }
    }
    
    public IPersistentValue<T> Initialize<T>(T defaultValue, Type? typeSerializationKey = null, 
        object? deserializationContext = null) where T : notnull
    {
        if (IsInitialized)
            throw new InvalidOperationException("This function can only be called on uninitialized data");

        var newValue = new PersistentValue<T>(defaultValue, typeSerializationKey, deserializationContext, this,
            _serializer);
        _persistentValue = newValue;
        
        if (!UpdateValueFromEncoded())
        {
            if (_encodedValue is not null) 
                _exceptionHandler.OnException(new ErrorDecodingValueException());
            
            UpdateEncodedFromValue();
        }
        
        return newValue;
    }
    
    public void OnDeletion()
    {
        _persistentValue?.SetDataOwner(null);
    }

    public IPersistentValue<T> GetValue<T>() => _persistentValue switch
    {
        null => throw new ValueNotInitializedException(),
        IPersistentValue<T> typed => typed,
        _ => throw new InvalidCastException()
    };

    public void UpdateEncodedFromValue()
    {
        if (Owner.Transcoder is null)
        {
            return;
        }
        
        if (_persistentValue is null)
        {
            throw new ValueNotInitializedException();
        }

        _encodedValue = _persistentValue.GetEncoded(Owner.Transcoder);
        Owner.OnChildValueChanged();
    }
        
    private bool UpdateValueFromEncoded()
    {
        if (Owner.Transcoder is null || _encodedValue is null || _persistentValue is null)
        {
            return false;
        }
        
        try
        {
            return _persistentValue.TrySetFromEncoded(_encodedValue, Owner.Transcoder);
        }
        catch (DeserializationError exception)
        {
            _exceptionHandler.OnException(exception);
            return false;
        }
    }
}