using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

internal interface IPersistentValueInternal
{
    public object GetEncoded(IPersistentDataTranscoder transcoder);

    public bool TrySetFromEncoded(object encodedValue, IPersistentDataTranscoder transcoder);
    
    public void Delete();
}