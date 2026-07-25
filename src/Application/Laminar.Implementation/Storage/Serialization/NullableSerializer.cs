using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class NullableSerializerFactory(ISerializer serializer) : IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(type.GetGenericArguments()[0]), serializer)
                as IConditionalSerializer;
        }

        return null;
    }
}

public class NullableSerializer<T>(ISerializer serializer) : TypeSerializer<T?> where T : struct
{
    public override Type SerializedType 
    {
        get
        {
            var serializedNotNullableType = serializer.GetSerializedType(typeof(T));

            if (serializedNotNullableType.IsClass || serializedNotNullableType.IsAbstract ||
                serializedNotNullableType.IsInterface)
            {
                return serializedNotNullableType;
            }
            
            return typeof(Nullable<>).MakeGenericType(serializedNotNullableType);
        }
    }
    
    protected override object? SerializeOverride(T? toSerialize) 
        => toSerialize is null ? null : serializer.SerializeObject(toSerialize.Value);

    protected override T? DeSerializeOverride(DeserializationRequest request)
    {
        if (request.Serialized is not { } notNullSerializedValue) return null;
        return serializer.DeserializeObject(request with
        {
            Serialized = notNullSerializedValue,
            TargetType = typeof(T),
            ExistingInstance = request.ExistingInstance is T notNull ? notNull : null
        }) is T deserialized ? deserialized : null;
    }
}