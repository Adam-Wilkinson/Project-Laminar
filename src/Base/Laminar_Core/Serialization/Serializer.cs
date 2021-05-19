using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Serialization
{
    public class Serializer : ISerializer
    {
        private static readonly Dictionary<Type, object> SerializableTypes = new();

        private readonly IObjectFactory _factory;

        public Serializer(IObjectFactory factory)
        {
            _factory = factory;
            if (SerializableTypes.Count == 0)
            {
                InitializeDictionaries();
            }
        }

        public ISerializedObject<T> Serialize<T>(T serializable)
        {
            if (!(SerializableTypes.TryGetValue(typeof(T), out object serializer) && serializer is IObjectSerializer<T> objectSerializer))
            {
                throw new NotImplementedException($"No serializable type found for type {typeof(T)}");
            }

            return objectSerializer.Serialize(serializable);
        }

        public IEnumerable<ISerializedObject<T>> Serialize<T>(IEnumerable<T> serializables)
        {
            foreach (T serializable in serializables)
            {
                yield return Serialize(serializable);
            }
        }

        public T Deserialize<T>(object serialized, object deserializationContext)
        {
            if (!(SerializableTypes.TryGetValue(typeof(T), out object serializer) && serialized is ISerializedObject<T> serializedObject && serializer is IObjectSerializer<T> objectSerializer))
            {
                throw new NotSupportedException($"Cannot deserialize objects of type {serialized.GetType()}. It does not implement ISerializedObject or a valid Serializer was not found");
            }

            return objectSerializer.DeSerialize(serializedObject, deserializationContext);
        }

        public IEnumerable<T> Deserialize<T>(IEnumerable<object> serializeds, object deserializationContext)
        {
            foreach (object serialized in serializeds)
            {
                yield return Deserialize<T>(serialized, deserializationContext);
            }
        }

        private void InitializeDictionaries()
        {
            foreach (Type type in typeof(Serializer).Assembly.GetTypes())
            {
                Type serializerType = type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IObjectSerializer<>)).FirstOrDefault();
                if (serializerType is not null)
                {
                    object serializer = _factory.CreateInstance(type);
                    type.GetProperty(nameof(IObjectSerializer<object>.Serializer)).SetValue(serializer, this);
                    Type genericParameter = serializerType.GetGenericArguments().FirstOrDefault();
                    SerializableTypes.Add(genericParameter, serializer);
                }
            }
        }
    }
}
