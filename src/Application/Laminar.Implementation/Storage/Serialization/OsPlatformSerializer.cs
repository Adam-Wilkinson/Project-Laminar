using System.Runtime.InteropServices;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class OsPlatformSerializer : TypeSerializer<OSPlatform, string>
{
    protected override string SerializeTyped(OSPlatform toSerialize) => toSerialize.ToString();

    protected override OSPlatform DeSerializeTyped(DeserializationRequest<OSPlatform, string> request) 
        => OSPlatform.Create(request.Serialized);
}