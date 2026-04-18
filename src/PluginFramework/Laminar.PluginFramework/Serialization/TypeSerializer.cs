namespace Laminar.PluginFramework.Serialization;

public abstract class TypeSerializer : IConditionalSerializer
{
    public abstract Type Type { get; }
    
    public abstract Type SerializedType { get; }
    
    public Type? SerializedTypeOrNull(Type typeToSerialize) => typeToSerialize == Type ? SerializedType : null;
    
    public abstract object Serialize(object toSerialize);

    public abstract object DeSerialize(object serialized, object? deserializationContext = null);
}

public abstract class TypeSerializer<T> : TypeSerializer
    where T : notnull
{
    public sealed override Type Type { get; } = typeof(T);
 
    public sealed override object Serialize(object toSerialize)
        => SerializeOverride((T)toSerialize);
    protected abstract object SerializeOverride(T toSerialize);
    
    public sealed override object DeSerialize(object serialized, object? context = null)
        => DeSerializeOverride(serialized, context); 
    protected abstract T DeSerializeOverride(object serialized, object? context = null);
}

public abstract class TypeSerializer<T, TSerialized> : TypeSerializer<T>
    where T : notnull where TSerialized : notnull
{
    public sealed override Type SerializedType { get; } = typeof(TSerialized);

    protected sealed override object SerializeOverride(T toSerialize) => SerializeTyped(toSerialize);
    
    protected abstract TSerialized SerializeTyped(T toSerialize);
    
    protected sealed override T DeSerializeOverride(object serialized, object? deserializationContext = null)
        => DeSerializeTyped((TSerialized)serialized, deserializationContext);
    
    protected abstract T DeSerializeTyped(TSerialized serialized, object? deserializationContext = null);
}