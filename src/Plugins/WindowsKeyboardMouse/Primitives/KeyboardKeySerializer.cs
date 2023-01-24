using System;
using Laminar.PluginFramework.Serialization;

namespace WindowsKeyboardMouse.Primitives;

public class KeyboardKeySerializer : IObjectSerializer<KeyboardKey>
{
    public KeyboardKey DeSerialize(ISerializedObject<KeyboardKey> serialized, ISerializer serializer, object deserializationContext)
    {
        if (serialized is not SerializedKeyboardKey serializedKeyboardKey)
        {
            throw new ArgumentException("Can only deserialize objects of type SerializedKeyboardKey");
        }

        return new KeyboardKey(serializedKeyboardKey.VirtualKey, serializedKeyboardKey.Modifiers);
    }

    public ISerializedObject<KeyboardKey> Serialize(KeyboardKey toSerialize, ISerializer serializer)
    {
        return new SerializedKeyboardKey(toSerialize.VirtualKey, toSerialize.Modifiers);
    }
}

public record SerializedKeyboardKey(int VirtualKey, KeyModifiers Modifiers) : ISerializedObject<KeyboardKey>;
