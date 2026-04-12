using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Notification;

namespace Laminar.Contracts.UserData.FileNavigation;

/// <summary>
/// A high-level file browser that acts on <see cref="ILaminarStorageItem"/> abstractions and pushes all changes
/// through the UserAction system
/// </summary>
public interface ILaminarFileBrowser
{
    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; }
    
    public Task<IUserActionResult> AddDefault<T>(ILaminarStorageFolder parentFolder)
        where T : class, ILaminarStorageItem;
    
    public Task<IUserActionResult> Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex);

    public Task<IUserActionResult> Delete(ILaminarStorageItem itemToDelete);

    public Task<IUserActionResult> Rename(ILaminarStorageItem itemToRename, string newName);
    
    public bool OpenInSystemFileBrowser(ILaminarStorageItem item);
}