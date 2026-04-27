using System;
using System.Resources;
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
        Owner.TranscoderChanged += (_, _) => InvalidateEncodedValue();
    }

    public PersistentDataNode Owner { get; }

    public DataPointState State { get; private set; }

    public object EncodedValue
    {
        get
        {
            if (_encodedValue is not null) return _encodedValue;
            
            if (Owner.Transcoder is null || State is not DataPointState.Active || _persistentValue is null)
            {
                throw new InvalidOperationException();
            }

            _encodedValue = _persistentValue.GetEncoded(Owner.Transcoder);
            return _encodedValue;
        }
        set
        {
            if (Equals(value, _encodedValue)) return;
            _encodedValue = value;

            if (State is not DataPointState.Active) return;
            
            if (!UpdateValueFromEncoded())
            {
                _exceptionHandler.OnException(new ErrorDecodingValueException());
                InvalidateEncodedValue();
            }
        }
    }
    
    public IPersistentValue<T> SetDefaultAndGet<T>(T defaultValue, Type? serializationKeyOverride = null, 
        object? deserializationContext = null) where T : notnull
    {
        var newValue = Initialize(() => new PersistentValue<T>(defaultValue, serializationKeyOverride,
            deserializationContext, this, _serializer));
        
        if (!UpdateValueFromEncoded())
        {
            if (_encodedValue is not null) 
                _exceptionHandler.OnException(new ErrorDecodingValueException());

            InvalidateEncodedValue();
        }
        
        return newValue;
    }

    public IPersistentValue<T> GetValue<T>() where T : notnull
    {
        if (State == DataPointState.Deleted)
        {
            throw new InvalidOperationException("Cannot get deleted data point value");
        }
        
        if (State == DataPointState.Active)
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

        return Initialize(() => PersistentValue<T>.FromEncodedValue(_encodedValue, null,
            null, this, _serializer, Owner.Transcoder));
    }
    
    public void OnDeletion()
    {
        _persistentValue?.Delete();
        State = DataPointState.Deleted;
    }

    public void InvalidateEncodedValue()
    {
        if (State is not DataPointState.Active) return;
        _encodedValue = null;
        Owner.OnChildValueInvalidated();
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

    private T Initialize<T>(Func<T> initializer) where T : IPersistentValueInternal
    {
        if (State is not DataPointState.Uninitialized)
            throw new InvalidOperationException("This function can only be called on uninitialized data");
        
        Owner.ChildIsInitializing = true;
        
        T value = initializer();
        _persistentValue = value;

        State = DataPointState.Active;
        Owner.ChildIsInitializing = false;
        return value;
    }
}