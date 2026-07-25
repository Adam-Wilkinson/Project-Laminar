namespace Laminar.Contracts.Storage.PersistentData;

public interface IEncodableData
{
    public object Encode(IPersistentDataTranscoder transcoder);
    
    public void Decode(IPersistentDataTranscoder transcoder, object? encoded);

    public event EventHandler? Invalidated;
}