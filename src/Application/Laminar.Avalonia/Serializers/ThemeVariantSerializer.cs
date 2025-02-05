using System;
using Avalonia.Styling;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.Serializers;

public class ThemeVariantSerializer : TypeSerializer<ThemeVariant, string>
{
    protected override string SerializeTyped(ThemeVariant toSerialize) => (string)toSerialize.Key;

    protected override ThemeVariant DeSerializeTyped(string serialized, object? deserializationContext = null) =>
        serialized switch
        {
            nameof(ThemeVariant.Dark) => ThemeVariant.Dark,
            nameof(ThemeVariant.Light) => ThemeVariant.Light,
            nameof(ThemeVariant.Default) => ThemeVariant.Default,
            _ => throw new ArgumentException($"Unknown theme variant {serialized}"),
        };
}