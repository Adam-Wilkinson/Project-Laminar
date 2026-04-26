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

    public PersistentDataPoint(PersistentDataNode owner,
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

    public PersistentDataNode Owner { get; }

    public bool IsInitialized { get; private set; }

    public object EncodedValue
    {
        get => _encodedValue ?? throw new InvalidCastException();
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
    
    public IPersistentValue<T> SetDefaultAndGet<T>(T defaultValue, Type? serializationKeyOverride = null, 
        object? deserializationContext = null) where T : notnull
    {
        if (IsInitialized)
            throw new InvalidOperationException("This function can only be called on uninitialized data");

        Owner.ChildIsInitializing = true;
        
        var newValue = new PersistentValue<T>(defaultValue, serializationKeyOverride, deserializationContext, this,
            _serializer);
        _persistentValue = newValue;
        _persistentValue.SetDataOwner(Owner);

        IsInitialized = true;
        Owner.ChildIsInitializing = false;
        
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

    public IPersistentValue<T> GetValue<T>() where T : notnull
    {
        if (IsInitialized)
        {
            return _persistentValue as IPersistentValue<T> ?? throw new InvalidCastException();
        }

        if (_encodedValue is null)
        {
            throw new InvalidOperationException("Uninitialized values need an encoded value to be retrieved");
        }

        if (Owner.Transcoder is null)
        {
            throw new InvalidOperationException("Cannot read encoded value without a transcoder");
        }

        Owner.ChildIsInitializing = true;
        
        var newValue = PersistentValue<T>.FromEncodedValue(_encodedValue, null,
            null, this, _serializer, Owner.Transcoder);
        _persistentValue = newValue;
        _persistentValue.SetDataOwner(Owner);
        
        IsInitialized = true;
        Owner.ChildIsInitializing = false;
        return newValue;
    }

    public void UpdateEncodedFromValue()
    {
        if (Owner.Transcoder is null || !IsInitialized || _persistentValue is null)
        {
            return;
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