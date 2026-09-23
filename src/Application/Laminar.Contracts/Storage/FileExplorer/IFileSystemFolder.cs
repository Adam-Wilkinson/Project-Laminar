using Laminar.Domain.Observables.Collections;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileSystemFolder : IFileSystemItem
{
    public IReadOnlyObservableList<IFileSystemItem>? Contents { get; }

    public IReadOnlyObservableList<IFileSystemItem> GetOrLoadContents();

    public Task<IReadOnlyObservableList<IFileSystemItem>> GetOrLoadContentsAsync();
    
    public bool IsExpanded { get; set; }
}