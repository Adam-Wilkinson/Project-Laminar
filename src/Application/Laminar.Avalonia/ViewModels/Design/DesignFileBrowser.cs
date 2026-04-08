using System.Collections.ObjectModel;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;

namespace Laminar.Avalonia.ViewModels.Design;

public class DesignFileBrowser : ILaminarFileBrowser
{
    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; } 
        = new ObservableCollection<ILaminarStorageRootFolder>().ToInterfaceImpl();

    public IUserActionResult AddDefault<T>(ILaminarStorageFolder parentFolder)
        where T : class, ILaminarStorageItem
        => IUserActionResult.Failure();

    public IUserActionResult Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex) 
        => IUserActionResult.Failure();

    public IUserActionResult Delete(ILaminarStorageItem itemToDelete) 
        => IUserActionResult.Failure();

    public IUserActionResult Rename(ILaminarStorageItem itemToRename, string newName) 
        => IUserActionResult.Failure();

    public bool OpenInSystemFileBrowser(ILaminarStorageItem item) => false;
}