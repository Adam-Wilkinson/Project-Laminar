using System;
using System.Collections.Generic;
using System.Linq;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Base.Serialization;

public class Serializer : ISerializer
{
    private static readonly Dictionary<Type, object> Serializers = new();

    public Serializer()
    {
        if (Serializers.Count == 0)
        {
            InitializeDictionaries();
        }
    }

    public ISerializedObject<T> Serialize<T>(T serializable)
    {
        if (!(Serializers.TryGetValue(typeof(T), out object serializer) && serializer is IObjectSerializer<T> objectSerializer))
        {
            throw new NotImplementedException($"No serializable type found for type {typeof(T)}");
        }

        return objectSerializer.Serialize(serializable, this);
    }

    public T Deserialize<T>(ISerializedObject<T> serialized, object deserializationContext)
    {
        if (!(Serializers.TryGetValue(typeof(T), out object serializer) && serializer is IObjectSerializer<T> objectSerializer))
        {
            throw new NotSupportedException($"Cannot deserialize objects of type {serialized.GetType()}. It does not implement ISerializedObject or a valid Serializer was not found");
        }

        return objectSerializer.DeSerialize(serialized, this, deserializationContext);
    }

    public void RegisterSerializer<T>(IObjectSerializer<T> serializer)
    {
        Serializers.Add(typeof(T), serializer);
    }

    public object TrySerializeObject(object toSerialize)
    {
        if (Serializers.TryGetValue(toSerialize.GetType(), out object serializer))
        {
            return typeof(IObjectSerializer<>).MakeGenericType(toSerialize.GetType()).GetMethod(nameof(IObjectSerializer<object>.Serialize)).Invoke(serializer, new object[] { toSerialize, this });
        }

        return toSerialize;
    }

    public object TryDeserializeObject(object serialized, Type requestedType, object deserializationContext)
    {
        if (requestedType is not null)
        {
            if (requestedType.IsEnum)
            {
                return Enum.ToObject(requestedType, serialized);
            }
        }

        Type deserializedType = serialized.GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISerializedObject<>)).FirstOrDefault()?.GetGenericArguments()[0];
        if (deserializedType is not null && Serializers.TryGetValue(deserializedType, out object serializer))
        {
            return typeof(IObjectSerializer<>).MakeGenericType(deserializedType).GetMethod(nameof(IObjectSerializer<object>.DeSerialize)).Invoke(serializer, new object[] { serialized, this, deserializationContext });
        }


        if (requestedType is not null)
        {
        }

        return serialized;
    }

    private void InitializeDictionaries()
    {
        foreach (Type type in typeof(Serializer).Assembly.GetTypes())
        {
            Type serializerType = type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IObjectSerializer<>)).FirstOrDefault();
            if (serializerType is not null)
            {
                //object serializer = _factory.CreateInstance(type);
                //Type genericParameter = serializerType.GetGenericArguments().FirstOrDefault();
                //Serializers.Add(genericParameter, serializer);
            }
        }
    }
}
