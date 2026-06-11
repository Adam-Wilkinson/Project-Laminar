using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class TimeSpanSerializer : TypeSerializer<TimeSpan, string>
{
    protected override string SerializeTyped(TimeSpan toSerialize) => toSerialize.ToString();

    protected override TimeSpan DeSerializeTyped(DeserializationRequest<TimeSpan, string> request) =>
        TimeSpan.TryParse(request.Serialized, out var result) ? result : throw new FormatException();
}