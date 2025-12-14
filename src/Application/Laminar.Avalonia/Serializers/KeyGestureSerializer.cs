using Avalonia.Input;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.Serializers;

public class KeyGestureSerializer : TypeSerializer<KeyGesture, string>
{
    protected override string SerializeTyped(KeyGesture toSerialize) => toSerialize.ToString();

    protected override KeyGesture DeSerializeTyped(string serialized, object? deserializationContext = null) =>
        KeyGesture.Parse(serialized);
}