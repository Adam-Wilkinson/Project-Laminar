using System;
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
        => throw new InvalidOperationException();

    public IUserActionResult Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex,
        IActionScope? scope = null)
    {
        return IUserActionResult.Failure();
    }

    public IUserActionResult Delete<T>(T itemToDelete, IActionScope? scope = null) where T : class, ILaminarStorageItem
    {
        return IUserActionResult.Failure();
    }

    public IUserActionResult Rename(ILaminarStorageItem itemToRename, string newName, IActionScope? scope = null)
    {
        return IUserActionResult.Failure();
    }

    public bool OpenInSystemFileBrowser(ILaminarStorageItem item)
    {
        return false;
    }
}