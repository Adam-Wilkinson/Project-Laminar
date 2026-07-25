using Laminar.Domain.Notification.Collections;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileSystemFolder : IFileSystemItem
{
    public IReadOnlyObservableCollection<IFileSystemItem>? Contents { get; }

    public IReadOnlyObservableCollection<IFileSystemItem> GetOrLoadContents();

    public Task<IReadOnlyObservableCollection<IFileSystemItem>> GetOrLoadContentsAsync();
    
    public bool IsExpanded { get; set; }
}