using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IFileSyncedResource<out T> : IDisposable where T : class, IEncodableDataOwner
{
    public T Resource { get; }
    
    public IPersistentDataTranscoder Transcoder { get; set; }
    
    public FileSystemPath Location { get; set; }
    
    public event EventHandler? OnDisposed;
}