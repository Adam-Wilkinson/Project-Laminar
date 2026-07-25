using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer;

/// <summary>
/// A high-level file browser that acts on <see cref="IFileSystemItem"/> abstractions and pushes all changes
/// through the UserAction system
/// </summary>
public interface IFileBrowser
{
    public IReadOnlyObservableCollection<IFileSystemRootFolder> RootFolders { get; }
    
    public Task<IUserActionResult> Add(string itemName, IFileSystemFolder parent, int indexInParent, FileSystemItemType type);
    
    public Task<IUserActionResult> Move(IFileSystemItem itemToMove, IFileSystemFolder destinationFolder, int destinationIndex);

    public Task<IUserActionResult> Delete(IFileSystemItem itemToDelete);

    public Task<IUserActionResult> Rename(IFileSystemItem itemToRename, string newName);
    
    public bool OpenInSystemFileBrowser(IFileSystemItem item);
    Task<IUserActionResult> RemoveRootFolder(FileSystemPath rootFolderPath);
    Task<IUserActionResult> AddRootFolder(FileSystemPath newRootFolderPath);
}