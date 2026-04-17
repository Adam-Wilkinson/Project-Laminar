using System;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataValue(ISerializer serializer, ILogger<PersistentDataValue> logger) 
    : ObservableValueBase<object>, IPersistentDataValue
{
    private object? _value;
    private object? _defaultValue;
    private object? _encodedValue;
    private object? _deserializationContext;
    private Type? _typeSerializationKey;
    
    public Type TypeSerializationKey => _typeSerializationKey ?? throw new ValueNotInitializedException(Name);
    
    public bool IsInitialized => _value is not null;

    public required string Name { get; init; }

    public void Reset()
    {
        if (!IsInitialized) throw new ValueNotInitializedException(Name);
        Value = DefaultValue;
    }

    public IPersistentDataValueOwner? Owner
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            field = value;
            
            if (IsInitialized)
            {
                SetEncodedValueFromValue();
            }
            
            if (Value is PersistentDataNode node)
            {
                node.Owner = Owner;
            }
        }
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
                node.Owner = Owner;
            }
            
            SetEncodedValueFromValue();
        }
    }

    public object? EncodedValue
    {
        get => _encodedValue;
        set
        {
            if (Equals(value, _encodedValue)) return;
            _encodedValue = value;

            if (!TrySetValueFromEncodedValue())
            {
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
            
        if (EncodedValue is not null && TrySetValueFromEncodedValue()) return;
            
        Value = DefaultValue;
    }

    protected override void OnValueChanged()
    {
        Owner?.OnChildChanged();
    }

    private void SetEncodedValueFromValue()
    {
        if (Owner?.Transcoder is null)
        {
            return;
        }
        
        if (_value is null)
        {
            throw new ValueNotInitializedException(Name);
        }
            
        var serialized = serializer.SerializeObject(_value, TypeSerializationKey);
        _encodedValue = Owner.Transcoder.EncodeValue(serialized);
    }
        
    private bool TrySetValueFromEncodedValue()
    {
        if (Owner?.Transcoder is null || EncodedValue is null)
        {
            return false;
        }
        
        if (Owner.Transcoder.DecodeValue(EncodedValue, serializer.GetSerializedType(TypeSerializationKey)) is not { } decodedValue)
        {
            logger.LogError("Error reading value {valueName}. Unable to decode value, the value will not be changed", Name);
            return false;
        }

        var newValue = serializer.DeserializeObject(decodedValue, TypeSerializationKey, _deserializationContext);
        if (_value is not null)
        {
            return SetAndRaise(ref _value, newValue);
        }

        _value = newValue;
        return true;
    }
}