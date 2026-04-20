using Avalonia.Input;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.Serializers;

public class KeyGestureSerializer : TypeSerializer<KeyGesture, string>
{
    protected override string SerializeTyped(KeyGesture toSerialize) => toSerialize.ToString();

    protected override KeyGesture DeSerializeTyped(DeserializationRequest<KeyGesture, string> request) =>
        KeyGesture.Parse(request.Serialized);
}