using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class PrimitiveSerializer : IConditionalSerializer
{
    public Type? SerializedTypeOrNull(Type typeToSerialize) =>
        typeToSerialize.IsPrimitive || typeToSerialize == typeof(string) ? typeToSerialize : null;

    public object Serialize(object toSerialize) => toSerialize;

    public object DeSerialize(DeserializationRequest request) => request.Serialized;
}