using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class PointSerializer : TypeSerializer<Point, string>
{
    protected override string SerializeTyped(Point toSerialize) => toSerialize.ToString();

    protected override Point DeSerializeTyped(DeserializationRequest<Point, string> request) => Point.Parse(request.Serialized);
}