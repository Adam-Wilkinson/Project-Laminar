using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class FileSystemPathSerializer : TypeSerializer<FileSystemPath, string>
{
    protected override string SerializeTyped(FileSystemPath toSerialize) 
        => toSerialize.ToString();

    protected override FileSystemPath DeSerializeTyped(DeserializationRequest<FileSystemPath, string> request) 
        => new(request.Serialized);
}