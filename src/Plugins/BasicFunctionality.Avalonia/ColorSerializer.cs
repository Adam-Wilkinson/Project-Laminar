using Avalonia.Media;
using Laminar.PluginFramework.Serialization;

namespace BasicFunctionality.Avalonia;

public class AvaloniaColorSerializer : TypeSerializer<Color, string>
{
    protected override string SerializeTyped(Color toSerialize) => toSerialize.ToString();

    protected override Color DeSerializeTyped(DeserializationRequest<Color, string> request) 
        => Color.Parse(request.Serialized);
}