using Laminar.Domain.DataManagement;

namespace Laminar.Contracts.UserData;

public interface IPersistentDataTranscoder
{
    public string FileExtension { get; }

    public byte[] Encode(Dictionary<string, IPersistentDataValue> toEncode);
    
    public Dictionary<string, IPersistentDataValue> Decode(byte[] data, Dictionary<string, IPersistentDataValue> typeHints);
}