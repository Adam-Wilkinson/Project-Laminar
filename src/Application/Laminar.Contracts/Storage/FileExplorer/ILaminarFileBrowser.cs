using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Notification;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer;

/// <summary>
/// A high-level file browser that acts on <see cref="ILaminarStorageItem"/> abstractions and pushes all changes
/// through the UserAction system
/// </summary>
public interface ILaminarFileBrowser
{
    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; }
    
    public Task<IUserActionResult> Add(string itemName, ILaminarStorageFolder parent, int indexInParent, StorageItemType type);
    
    public Task<IUserActionResult> Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex);

    public Task<IUserActionResult> Delete(ILaminarStorageItem itemToDelete);

    public Task<IUserActionResult> Rename(ILaminarStorageItem itemToRename, string newName);
    
    public bool OpenInSystemFileBrowser(ILaminarStorageItem item);
    Task<IUserActionResult> RemoveRootFolder(FileSystemPath rootFolderPath);
    Task<IUserActionResult> AddRootFolder(FileSystemPath newRootFolderPath);
}