using System;
using Avalonia.Themes.Fluent;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.Serializers;

public class DensityStyleSerializer : TypeSerializer<DensityStyle, string>
{
    protected override string SerializeTyped(DensityStyle toSerialize) => toSerialize.ToString();

    protected override DensityStyle DeSerializeTyped(string serialized, object? deserializationContext = null) =>
        serialized switch
        {
            nameof(DensityStyle.Compact) => DensityStyle.Compact,
            nameof(DensityStyle.Normal) => DensityStyle.Normal,
            _ => throw new ArgumentException($"Unknown density style {serialized}"),
        };
}