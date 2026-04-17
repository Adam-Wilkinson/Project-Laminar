using System;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class TimeSpanSerializer : TypeSerializer<TimeSpan, string>
{
    protected override string SerializeTyped(TimeSpan toSerialize) => toSerialize.ToString();

    protected override TimeSpan DeSerializeTyped(string serialized, object? deserializationContext = null) =>
        TimeSpan.TryParse(serialized, out var result) ? result : throw new FormatException();
}