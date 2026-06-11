using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Base.UserInterface;

public class SourcedInterfaceDataSerializerFactory : IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SourcedInterfaceData<>))
        {
            return Activator.CreateInstance(typeof(SourcedInterfaceDataSerializer<>).MakeGenericType(type)) as IConditionalSerializer;
        }

        return null;
    }
}

public class SourcedInterfaceDataSerializer<T> : TypeSerializer<SourcedInterfaceData<T>, T> where T : notnull
{
    protected override T SerializeTyped(SourcedInterfaceData<T> toSerialize)
    {
        throw new NotImplementedException();
    }

    protected override SourcedInterfaceData<T> DeSerializeTyped(DeserializationRequest<SourcedInterfaceData<T>, T> request)
    {
        throw new NotImplementedException();
    }

    protected override INotifySerializedValueChanged? GetSerializedValueChangedNotifier(T target)
    {
        throw new NotImplementedException();
    }
}