using System;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.PersistentData;
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

    public Type TypeSerializationKey => _typeSerializationKey ?? throw new ValueNotInitializedException();
    
    public bool IsInitialized => _value is not null;
    
    public void OnDeletion()
    {
        if (Value is PersistentDataNode node)
        {
            node.Owner = null;
        }
    }

    public void Reset()
    {
        if (!IsInitialized) throw new ValueNotInitializedException();
        Value = DefaultValue;
    }

    public override object Value
    {
        get => _value ?? throw new ValueNotInitializedException();
        set
        {
            if (_value is null)
            {
                throw new ValueNotInitializedException();
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
        get => _encodedValue ?? throw new ValueNotInitializedException();
        set
        {
            if (Equals(value, _encodedValue)) return;
            _encodedValue = value;

            if (!IsInitialized) return;
            
            if (!TrySetValueFromEncodedValue())
            {
                _exceptionHandler.OnException(new ErrorDecodingValueException(Value));
                SetEncodedValueFromValue();
            }
        }
    }
    
    public object DefaultValue => _defaultValue ?? throw new ValueNotInitializedException();
    
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
        
        _value = defaultValue;
        if (TrySetValueFromEncodedValue()) return;

        if (_encodedValue is not null) _exceptionHandler.OnException(new ErrorDecodingValueException(Value));
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
            throw new ValueNotInitializedException();
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

        if (_value is null)
        {
            throw new InvalidOperationException("Cannot set value without an existing value");
        }
        
        if (_owner.Transcoder.DecodeElement(EncodedValue, _serializer.GetSerializedType(_typeSerializationKey)) is not { } decodedValue)
        {
            _logger.LogError("Error reading value. Unable to decode value, the value will not be changed");
            return false;
        }

        object newValue;
        
        try
        {
            newValue = _serializer.DeserializeObject(new DeserializationRequest
            {
                Serialized = decodedValue,
                TargetType = TypeSerializationKey,
                ExistingInstance = _value,
                Context = _deserializationContext
            });
        }
        catch (DeserializationError)
        {
            return false;
        }
        
        if (_value is PersistentDataNode node)
        {
            node.Owner = _owner;
        }
        SetAndRaise(ref _value, newValue);
        return true;
    }
}