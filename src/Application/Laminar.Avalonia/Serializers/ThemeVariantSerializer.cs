using System;
using Avalonia.Styling;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.Serializers;

public class ThemeVariantSerializer : TypeSerializer<ThemeVariant, string>
{
    protected override string SerializeTyped(ThemeVariant toSerialize) => (string)toSerialize.Key;

    protected override ThemeVariant DeSerializeTyped(DeserializationRequest<ThemeVariant, string> request) =>
        request.Serialized switch
        {
            nameof(ThemeVariant.Dark) => ThemeVariant.Dark,
            nameof(ThemeVariant.Light) => ThemeVariant.Light,
            nameof(ThemeVariant.Default) => ThemeVariant.Default,
            var unknown => throw new ArgumentException($"Unknown theme variant {unknown}"),
        };
}