using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Serialization
{
    public class Serializer : ISerializer
    {
        private static readonly Dictionary<Type, object> Serializers = new();

        private readonly IObjectFactory _factory;

        public Serializer(IObjectFactory factory)
        {
            _factory = factory;
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

        public object TryDeserializeObject(object serialized, object deserializationContext)
        {
            Type deserializedType = serialized.GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISerializedObject<>)).FirstOrDefault()?.GetGenericArguments()[0];
            if (deserializedType is not null && Serializers.TryGetValue(deserializedType, out object serializer))
            {
                return typeof(IObjectSerializer<>).MakeGenericType(deserializedType).GetMethod(nameof(IObjectSerializer<object>.DeSerialize)).Invoke(serializer, new object[] { serialized, this, deserializationContext });
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
                    object serializer = _factory.CreateInstance(type);
                    Type genericParameter = serializerType.GetGenericArguments().FirstOrDefault();
                    Serializers.Add(genericParameter, serializer);
                }
            }
        }
    }
}
