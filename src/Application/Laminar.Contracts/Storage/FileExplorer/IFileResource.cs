using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileResource<out T> : IDisposable where T : IEncodableDataOwner<IEncodableData>
{
    public IFileSystemFile FileSystemFile { get; }
    
    public T Value { get; }

    public event EventHandler? OnDeleted;
}