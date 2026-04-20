using System;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class DefaultSerializerFactory(ISerializer serializer) : IConditionalSerializerFactory
{
    private readonly ISerializer _serializer = serializer;
    
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (type.IsPrimitive || type == typeof(string))
        {
            return new PrimitiveSerializer();
        }

        return null;
    }
}

public class PrimitiveSerializer : IConditionalSerializer
{
    public Type? SerializedTypeOrNull(Type typeToSerialize) => typeToSerialize;

    public object Serialize(object toSerialize) => toSerialize;

    public object DeSerialize(DeserializationRequest request) => request.Serialized;
}