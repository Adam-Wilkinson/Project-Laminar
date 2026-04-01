using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.UserData.Serializers;

public class FileSystemPathSerializer : TypeSerializer<FileSystemPath, string>
{
    protected override string SerializeTyped(FileSystemPath toSerialize) 
        => toSerialize.ToString();

    protected override FileSystemPath DeSerializeTyped(string serialized, object? deserializationContext = null) 
        => new(serialized);
}