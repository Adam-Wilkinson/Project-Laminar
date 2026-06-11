using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class PointSerializer : TypeSerializer<Point, string>
{
    protected override string SerializeTyped(Point toSerialize)
    {
        throw new NotImplementedException();
    }

    protected override Point DeSerializeTyped(DeserializationRequest<Point, string> request)
    {
        throw new NotImplementedException();
    }
}