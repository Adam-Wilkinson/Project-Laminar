using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class GuidIdentifierSerializerFactory : IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(GuidIdentifier<>))
        {
            return Activator.CreateInstance(typeof(GuidIdentifierSerializer<>).MakeGenericType(type.GetGenericArguments()[0])) as IConditionalSerializer;
        }

        return null;
    }
}

public class GuidIdentifierSerializer<T> : TypeSerializer<GuidIdentifier<T>, string> 
{
    protected override string SerializeTyped(GuidIdentifier<T> toSerialize) 
        => toSerialize.AsGuid().ToString();

    protected override GuidIdentifier<T> DeSerializeTyped(DeserializationRequest<GuidIdentifier<T>, string> request) 
        => new(Guid.Parse(request.Serialized));
}