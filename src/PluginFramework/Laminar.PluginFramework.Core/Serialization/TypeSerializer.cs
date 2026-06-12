namespace Laminar.PluginFramework.Serialization;

public abstract class TypeSerializer : INotifyingConditionalSerializer
{
    public abstract Type Type { get; }
    
    public abstract Type SerializedType { get; }
    
    public Type? SerializedTypeOrNull(Type typeToSerialize) => Type.IsAssignableFrom(typeToSerialize) ? SerializedType : null;
    
    public abstract object Serialize(object toSerialize);

    public abstract object DeSerialize(DeserializationRequest request);

    public virtual INotifySerializedValueChanged? GetSerializedValueChangedNotifier(object target) => null;
}

public abstract class TypeSerializer<T> : TypeSerializer where T : notnull
{
    public sealed override Type Type { get; } = typeof(T);
 
    public sealed override object Serialize(object toSerialize)
        => SerializeOverride((T)toSerialize);
    
    protected abstract object SerializeOverride(T toSerialize);
    
    public sealed override object DeSerialize(DeserializationRequest request) 
        => DeSerializeOverride(request);
    
    protected abstract T DeSerializeOverride(DeserializationRequest request);

    protected virtual INotifySerializedValueChanged? GetSerializedValueChangedNotifier(T target) => null;
    
    public sealed override INotifySerializedValueChanged? GetSerializedValueChangedNotifier(object target)
        => GetSerializedValueChangedNotifier((T)target);
}

public abstract class TypeSerializer<T, TSerialized> : TypeSerializer<T>
    where T : notnull where TSerialized : notnull
{
    public sealed override Type SerializedType { get; } = typeof(TSerialized);

    protected sealed override object SerializeOverride(T toSerialize) => 
        SerializeTyped(toSerialize);
    
    protected abstract TSerialized SerializeTyped(T toSerialize);
    
    protected sealed override T DeSerializeOverride(DeserializationRequest request)
        => DeSerializeTyped(new DeserializationRequest<T, TSerialized>(request));
    
    protected abstract T DeSerializeTyped(DeserializationRequest<T, TSerialized> request);
}