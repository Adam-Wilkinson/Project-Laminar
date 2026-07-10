using Laminar.Domain.Notification.Collections;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileSystemFolder : IFileSystemItem
{
    public IReadOnlyObservableCollection<IFileSystemItem>? Contents { get; }

    public IReadOnlyObservableCollection<IFileSystemItem> LoadOrGetContents();

    public bool IsExpanded { get; set; }
}