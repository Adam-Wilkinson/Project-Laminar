namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataValueOwner
{
    public IPersistentDataTranscoder? Transcoder { get; }

    public void OnChildChanged();
}