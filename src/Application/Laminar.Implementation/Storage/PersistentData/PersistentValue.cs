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
    private readonly T? _defaultValue = default;
    
    private T _value;
    
    public static PersistentValue<T> FromEncodedValue(
        object encodedValue,
        Type? typeSerializationKeyOverride, 
        object? deserializationContext,
        PersistentDataPoint parent,
        ISerializer serializer,
        IPersistentDataTranscoder transcoder)
    {
        T initialValue = GetValueFromEncoded(encodedValue, serializer, transcoder,
            typeSerializationKeyOverride ?? typeof(T), deserializationContext, null);
        
        return new PersistentValue<T>(initialValue, typeSerializationKeyOverride, deserializationContext, parent,
            serializer)
        {
            HasDefaultValue = false,
        };
    }
    
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
        _defaultValue = value;

        _serializer.GetSerializedValueChangedNotifier(_value, TypeSerializationKey).SerializedValueChanged += (_, _) =>
        {
            if (_parent.IsInitialized) _parent.UpdateEncodedFromValue();
        };
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

    public bool HasDefaultValue { get; private init; } = true;

    public T DefaultValue => _defaultValue ?? throw new Exception("This persistent value does not have a default");

    public void Reset()
    {
        if (_defaultValue is not null) Value = _defaultValue;
    }

    public object GetEncoded(IPersistentDataTranscoder transcoder)
    {
        var serialized = _serializer.SerializeObject(Value, TypeSerializationKey);
        return transcoder.EncodeElement(serialized) ?? throw new Exception();
    }

    public bool TrySetFromEncoded(object encodedValue, IPersistentDataTranscoder transcoder)
    {
        T newValue = GetValueFromEncoded(encodedValue, _serializer, transcoder, TypeSerializationKey,
            _deserializationContext, _value);
        
        SetAndRaise(ref _value, newValue);
        return true;
    }
    
    protected override void OnValueChanged()
    {
        SetDataOwner(_parent.Owner);
    }

    public void SetDataOwner(PersistentDataNode? newOwner)
    {
        switch (Value)
        {
            case PersistentDataNode node:
                SetOwner(node, newOwner);
                break;
            case IEnumerable<PersistentDataNode> nodes:
            {
                foreach (var eachNode in nodes)
                {
                    SetOwner(eachNode, newOwner);
                }

                break;
            }
        }
    }

    private void SetOwner(PersistentDataNode childNode, PersistentDataNode? newOwner)
    {
        // This can happen if the data node is added to a new tree before it is removed from the old one.
        // We do not want to nullify the new owner in this case
        if (newOwner is null && !ReferenceEquals(_parent.Owner, childNode.Owner)) return;
        
        (childNode.Owner as PersistentDataNode)?.RemoveChildNode(childNode);
        newOwner?.RegisterChildNode(childNode);
    }

    private static T GetValueFromEncoded(object encodedValue, ISerializer serializer, IPersistentDataTranscoder transcoder, Type typeSerializationKey,
        object? deserializationContext, object? existingValue)
    {
        var decoded = transcoder.DecodeElement(encodedValue, serializer.GetSerializedType(typeSerializationKey));

        if (decoded is null) throw new DeserializationError(new InvalidCastException());
        
        var newValue = serializer.DeserializeObject(new DeserializationRequest
        {
            Serialized = decoded,
            TargetType = typeSerializationKey,
            ExistingInstance = existingValue,
            Context = deserializationContext
        });

        if (newValue is not T typedValue)
        {
            throw new DeserializationError(new InvalidCastException());
        }

        return typedValue;
    }
}