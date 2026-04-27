namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataValueOwner
{
    public IPersistentDataTranscoder? Transcoder { get; }
    
    public event EventHandler? TranscoderChanged;
    
    public void OnChildValueInvalidated();
}