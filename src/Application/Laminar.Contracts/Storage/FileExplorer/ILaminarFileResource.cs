using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface ILaminarFileResource<out T> : IDisposable where T : IEncodableDataOwner<IEncodableData>
{
    public ILaminarStorageFile File { get; }
    
    public T Value { get; }

    public event EventHandler? OnDeleted;
}