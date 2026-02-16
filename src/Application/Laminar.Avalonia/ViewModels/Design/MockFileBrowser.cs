using System;
using System.Collections.ObjectModel;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Laminar.Implementation.UserData.FileNavigation.UserActions;

namespace Laminar.Avalonia.ViewModels.Design;

public class MockFileBrowser : ILaminarFileBrowser
{
    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; } 
        = new ObservableCollection<ILaminarStorageRootFolder>().ToInterfaceImpl();

    public bool AddDefault<T>(ILaminarStorageFolder parentFolder, IActionScope? scope = null)
        where T : class, ILaminarStorageItem
        => throw new InvalidOperationException();

    public bool Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex,
        IActionScope? scope = null)
    {
        destinationFolder.Contents.Insert(destinationIndex, itemToMove);
        return true;
    }

    public bool Delete<T>(T itemToDelete, IActionScope? scope = null) where T : class, ILaminarStorageItem
    {
        itemToDelete.Delete();
        return true;
    }

    public bool Rename(ILaminarStorageItem itemToRename, string newName, IActionScope? scope = null)
    {
        itemToRename.Name = newName;
        return true;
    }
}