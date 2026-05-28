using System.Reflection;

namespace Laminar.PluginFramework.Serialization;

public interface ISerializer
{
    public void EnsureAssemblyInit(Assembly assembly);
    
    public object SerializeObject(object toSerialize, Type? overrideTypeKey = null);

    public object DeserializeObject(DeserializationRequest request);
    
    public void RegisterSerializer(IConditionalSerializer serializer);
    
    public void RegisterFactory(IConditionalSerializerFactory factory);

    public Type GetSerializedType(Type typeToSerialize);

    public INotifySerializedValueChanged GetSerializedValueChangedNotifier(object target, Type overrideTypeKey);
}

public static class SerializerExtensions
{
    extension(ISerializer serializer)
    {
        public T? TryDeserialize<T>(object serialized, object? deserializationContext = null)
            => serializer.DeserializeObject(new DeserializationRequest
            {
                Serialized = serialized,
                TargetType = typeof(T),
                Context = deserializationContext,
            }) is T typed ? typed : default;

        public object Deserialize(object serialized, Type targetType) => serializer.DeserializeObject(
            new DeserializationRequest
            {
                Serialized = serialized,
                TargetType = targetType
            });
    }
}