namespace Laminar.PluginFramework.Serialization;

public interface IObjectSerializer<T>
{
    ISerializedObject<T> Serialize(T toSerialize, ISerializer serializer);

    T DeSerialize(ISerializedObject<T> serialized, ISerializer serializer, object deserializationContext);
}
