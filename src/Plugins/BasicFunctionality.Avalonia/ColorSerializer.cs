using Avalonia.Media;
using Laminar.PluginFramework.Serialization;

namespace BasicFunctionality.Avalonia;

public class AvaloniaColorSerializer : TypeSerializer<Color, string>
{
    protected override string SerializeTyped(Color toSerialize)
    {
        return toSerialize.ToString();
    }

    protected override Color DeSerializeTyped(string serialized, object? deserializationContext = null)
    {
        return Color.Parse(serialized);
    }
}