using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IDataOnDisk<out T> : IDisposable where T : class, IEncodableData
{
    public T Data { get; }
    
    public IPersistentDataTranscoder Transcoder { get; set; }
    
    public FileSystemPath Location { get; set; }
    
    public event EventHandler? OnDisposed;
}