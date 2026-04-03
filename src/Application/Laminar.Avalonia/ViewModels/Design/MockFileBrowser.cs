using System.Collections.ObjectModel;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;

namespace Laminar.Avalonia.ViewModels.Design;

public class MockFileBrowser : ILaminarFileBrowser
{
    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; } 
        = new ObservableCollection<ILaminarStorageRootFolder>().ToInterfaceImpl();

    public IUserActionResult AddDefault<T>(ILaminarStorageFolder parentFolder, IActionScope? scope = null)
        where T : class, ILaminarStorageItem
        => IUserActionResult.Failure();

    public IUserActionResult Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex,
        IActionScope? scope = null) 
        => IUserActionResult.Failure();

    public IUserActionResult Delete(ILaminarStorageItem itemToDelete, IActionScope? scope = null) 
        => IUserActionResult.Failure();

    public IUserActionResult Rename(ILaminarStorageItem itemToRename, string newName, IActionScope? scope = null) 
        => IUserActionResult.Failure();

    public bool OpenInSystemFileBrowser(ILaminarStorageItem item) => false;
}