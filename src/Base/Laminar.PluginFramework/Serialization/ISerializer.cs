using System;

namespace Laminar.PluginFramework.Serialization;

public interface ISerializer
{
    public object TrySerializeObject(object toSerialize);

    public object TryDeserializeObject(object serialized, Type? requestedType = null, object? deserializationContext = null);

    public void RegisterSerializer<T>(IObjectSerializer<T> serializer);

    public ISerializedObject<T> Serialize<T>(T serializable);

    public T Deserialize<T>(ISerializedObject<T> serialized, object deserializationContext);
}
