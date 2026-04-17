using System.Runtime.InteropServices;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class OsPlatformSerializer : TypeSerializer<OSPlatform, string>
{
    protected override string SerializeTyped(OSPlatform toSerialize) => toSerialize.ToString();

    protected override OSPlatform DeSerializeTyped(string serialized, object? deserializationContext = null) => OSPlatform.Create(serialized);
}