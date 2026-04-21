using System;
using System.Collections.Generic;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentValue<T> : ObservableValueBase<T>, IPersistentValue<T>, IPersistentValueInternal 
    where T : notnull
{
    private readonly ISerializer _serializer;
    private readonly PersistentDataPoint _parent;
    private readonly object? _deserializationContext;
    
    private T _value;

    public PersistentValue(
        T value,
        Type? typeSerializationKeyOverride,
        object? deserializationContext,
        PersistentDataPoint parent,
        ISerializer serializer)
    {
        _value = value;
        TypeSerializationKey = typeSerializationKeyOverride ?? typeof(T);
        _deserializationContext = deserializationContext;
        _parent = parent;
        _serializer = serializer;
        DefaultValue = value;

        _serializer.GetSerializedValueChangedNotifier(_value, TypeSerializationKey).SerializedValueChanged += (_, _) =>
        {
            _parent.UpdateEncodedFromValue();
        };
        
        SetDataOwner(_parent.Owner);
    }

    public Type TypeSerializationKey { get; }

    public override T Value
    {
        get => _value;
        set
        {
            if (!SetAndRaise(ref _value, value)) return;
            
            _parent.UpdateEncodedFromValue();
        }
    }
    
    public T DefaultValue { get; }

    public void Reset() => Value = DefaultValue;

    public object GetEncoded(IPersistentDataTranscoder transcoder)
    {
        var serialized = _serializer.SerializeObject(Value, TypeSerializationKey);
        return transcoder.EncodeElement(serialized) ?? throw new Exception();
    }

    public bool TrySetFromEncoded(object encodedValue, IPersistentDataTranscoder transcoder)
    {
        var decoded = transcoder.DecodeElement(encodedValue, _serializer.GetSerializedType(TypeSerializationKey));

        if (decoded is null) return false;
        
        var newValue = _serializer.DeserializeObject(new DeserializationRequest
        {
            Serialized = decoded,
            TargetType = TypeSerializationKey,
            ExistingInstance = _value,
            Context = _deserializationContext
        });

        if (newValue is not T typedValue)
        {
            throw new DeserializationError(new InvalidCastException());
        }
        
        SetAndRaise(ref _value, typedValue);
        return true;
    }
    
    protected override void OnValueChanged()
    {
        SetDataOwner(_parent.Owner);
    }

    public void SetDataOwner(IPersistentDataValueOwner? newOwner)
    {
        switch (Value)
        {
            case PersistentDataNode node:
                node.Owner = newOwner;
                break;
            case IEnumerable<PersistentDataNode> nodes:
            {
                foreach (var eachNode in nodes)
                {
                    eachNode.Owner = newOwner;
                }

                break;
            }
        }
    }
}