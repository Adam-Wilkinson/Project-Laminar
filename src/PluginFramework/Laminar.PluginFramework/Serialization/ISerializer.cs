using System.Reflection;

namespace Laminar.PluginFramework.Serialization;

public interface ISerializer
{
    public void EnsureAssemblyInit(Assembly assembly);
    
    public object SerializeObject(object toSerialize, Type? overrideTypeKey = null);

    public object DeserializeObject(object serialized, Type requestedType, object? context = null);
    
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
            => serializer.DeserializeObject(serialized, typeof(T), deserializationContext) is T typed ? typed : default;
    }
}