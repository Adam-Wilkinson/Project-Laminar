using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataValue : ObservableValueBase<object>, IPersistentDataValue
{
    private readonly ISerializer _serializer;
    private readonly ILogger<PersistentDataValue> _logger;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IPersistentDataValueOwner _owner;
    
    private object? _value;
    private object? _defaultValue;
    private object? _encodedValue;
    private object? _deserializationContext;
    private Type? _typeSerializationKey;

    public PersistentDataValue(IPersistentDataValueOwner owner,
        ISerializer serializer, 
        IExceptionHandler exceptionHandler,
        ILogger<PersistentDataValue> logger)
    {
        _serializer = serializer;
        _logger = logger;
        _owner = owner;
        _exceptionHandler = exceptionHandler;
        _owner.TranscoderChanged += (_, _) =>
        {
            if (IsInitialized) SetEncodedValueFromValue();
        };
    }

    public Type TypeSerializationKey => _typeSerializationKey ?? throw new ValueNotInitializedException(Name);
    
    public bool IsInitialized => _value is not null;

    public required string Name { get; init; }

    public void Reset()
    {
        if (!IsInitialized) throw new ValueNotInitializedException(Name);
        Value = DefaultValue;
    }

    public override object Value
    {
        get => _value ?? throw new ValueNotInitializedException(Name);
        set
        {
            if (_value is null)
            {
                throw new ValueNotInitializedException(Name);
            }
            
            if (!SetAndRaise(ref _value, value)) return;
            
            // We just had a valid update of value
            if (Value is PersistentDataNode node)
            {
                node.Owner = _owner;
            }
            
            SetEncodedValueFromValue();
        }
    }

    public object EncodedValue
    {
        get => _encodedValue ?? throw new ValueNotInitializedException(Name);
        set
        {
            if (Equals(value, _encodedValue)) return;
            _encodedValue = value;

            if (!IsInitialized) return;
            
            if (!TrySetValueFromEncodedValue())
            {
                _exceptionHandler.OnException(new ErrorDecodingValueException(Name, Value));
                SetEncodedValueFromValue();
            }
        }
    }
    
    public object DefaultValue => _defaultValue ?? throw new ValueNotInitializedException(Name);
    
    public void Initialize(object defaultValue, Type? typeSerializationKey = null, object? deserializationContext = null)
    {
        if (IsInitialized)
            throw new InvalidOperationException("This function can only be called on uninitialized data");
        
        _defaultValue = defaultValue;
        _typeSerializationKey = typeSerializationKey ?? DefaultValue.GetType();
        _deserializationContext = deserializationContext;
            
        _serializer.GetSerializedValueChangedNotifier(defaultValue, TypeSerializationKey).SerializedValueChanged +=
        (_, _) =>
        {
            SetEncodedValueFromValue();
        };
        
        if (TrySetValueFromEncodedValue()) return;

        _value = defaultValue;
        _exceptionHandler.OnException(new ErrorDecodingValueException(Name, Value));
        if (Value is PersistentDataNode node)
        {
            node.Owner = _owner;
        }
            
        SetEncodedValueFromValue();
    }

    private void SetEncodedValueFromValue()
    {
        if (_owner.Transcoder is null)
        {
            return;
        }
        
        if (_value is null)
        {
            throw new ValueNotInitializedException(Name);
        }
            
        var serialized = _serializer.SerializeObject(_value, TypeSerializationKey);
        _encodedValue = _owner.Transcoder.EncodeElement(serialized);
        _owner.OnChildValueChanged();
    }
        
    private bool TrySetValueFromEncodedValue()
    {
        if (_owner.Transcoder is null || _encodedValue is null || _typeSerializationKey is null)
        {
            return false;
        }
        
        if (_owner.Transcoder.DecodeElement(EncodedValue, _serializer.GetSerializedType(_typeSerializationKey)) is not { } decodedValue)
        {
            _logger.LogError("Error reading value {valueName}. Unable to decode value, the value will not be changed", Name);
            return false;
        }

        object newValue;
        
        try
        {
            newValue = _serializer.DeserializeObject(decodedValue, TypeSerializationKey, _deserializationContext);
        }
        catch (DeserializationError)
        {
            return false;
        }
        
        if (_value is not null)
        {
            return SetAndRaise(ref _value, newValue);
        }

        _value = newValue;
        if (_value is PersistentDataNode node)
        {
            node.Owner = _owner;
        }
        return true;
    }
}