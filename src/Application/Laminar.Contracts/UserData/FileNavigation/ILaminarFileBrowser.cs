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
    
    public IUserActionResult AddDefault<T>(ILaminarStorageFolder parentFolder, IActionScope? scope = null)
        where T : class, ILaminarStorageItem;
    
    public IUserActionResult Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex,
        IActionScope? scope = null);

    public IUserActionResult Delete<T>(T itemToDelete, IActionScope? scope = null) where T : class, ILaminarStorageItem;

    public IUserActionResult Rename(ILaminarStorageItem itemToRename, string newName, IActionScope? scope = null);
    
    public bool OpenInSystemFileBrowser(ILaminarStorageItem item);
}