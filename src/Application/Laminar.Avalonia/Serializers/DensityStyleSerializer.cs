using System;
using Avalonia.Themes.Fluent;
using Laminar.Domain.Exceptions;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.Serializers;

public class DensityStyleSerializer : TypeSerializer<DensityStyle, string>
{
    protected override string SerializeTyped(DensityStyle toSerialize) => toSerialize.ToString();

    protected override DensityStyle DeSerializeTyped(DeserializationRequest<DensityStyle, string> request) =>
        request.Serialized switch
        {
            nameof(DensityStyle.Compact) => DensityStyle.Compact,
            nameof(DensityStyle.Normal) => DensityStyle.Normal,
            var unknown => throw new DeserializationError(new ArgumentException($"Unknown density style {unknown}")),
        };
}