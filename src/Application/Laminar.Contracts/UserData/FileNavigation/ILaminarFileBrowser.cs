using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Notification;

namespace Laminar.Contracts.UserData.FileNavigation;

public interface ILaminarFileBrowser
{
    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; }

    public bool AddDefault<T>(ILaminarStorageFolder parentFolder, IActionScope? scope = null)
        where T : class, ILaminarStorageItem;
    
    public bool Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex,
        IActionScope? scope = null);

    public bool Delete<T>(T itemToDelete, IActionScope? scope = null) where T : class, ILaminarStorageItem;

    public bool Rename(ILaminarStorageItem itemToRename, string newName, IActionScope? scope = null);
    
    public bool OpenInSystemFileBrowser(ILaminarStorageItem item);
}