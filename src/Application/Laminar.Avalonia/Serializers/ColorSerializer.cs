using Avalonia.Media;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.Serializers;

public class ColorSerializer : TypeSerializer<Color, string>
{
    protected override string SerializeTyped(Color toSerialize) => toSerialize.ToString();

    protected override Color DeSerializeTyped(DeserializationRequest<Color, string> request) 
        => Color.Parse(request.Serialized);
}