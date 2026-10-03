using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer;

/// <summary>
/// A high-level file browser that acts on <see cref="IFileSystemItem"/> abstractions and pushes all changes
/// through the UserAction system
/// </summary>
public interface IFileBrowser
{
    public IReadOnlyObservableList<IFileSystemRootFolder> RootFolders { get; }

    public IUserActionScope ActionScope { get; }

    public Task<UserActionResult> Add(string itemName, IFileSystemFolder parent, int indexInParent, FileSystemItemType type);
    
    public Task<UserActionResult> Move(IFileSystemItem itemToMove, IFileSystemFolder destinationFolder, int destinationIndex);

    public Task<UserActionResult> Delete(IFileSystemItem itemToDelete);

    public Task<UserActionResult> Rename(IFileSystemItem itemToRename, string newName);
    
    public bool OpenInSystemFileBrowser(IFileSystemItem item);
    
    Task<UserActionResult> RemoveRootFolder(FileSystemPath rootFolderPath);
    
    Task<UserActionResult> AddRootFolder(FileSystemPath newRootFolderPath);
}