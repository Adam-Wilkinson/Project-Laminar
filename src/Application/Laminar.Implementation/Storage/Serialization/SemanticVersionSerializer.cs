using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class SemanticVersionSerializer : TypeSerializer<SemanticVersion, string>
{
    protected override string SerializeTyped(SemanticVersion toSerialize) => toSerialize.ToString();

    protected override SemanticVersion DeSerializeTyped(DeserializationRequest<SemanticVersion, string> request) =>
        new(request.Serialized);
}