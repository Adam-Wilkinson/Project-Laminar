using System;
using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.Domain.Extensions;
using Laminar.Implementation.UserData.FileNavigation.UserActions;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;


public class LaminarFileBrowser : ILaminarFileBrowser, IDisposable
{
    private readonly IUserActionManager _actionManager;
    private readonly ILaminarStorageItemFactory _factory;
    private readonly IFileSystem _fileSystem;

    public LaminarFileBrowser(IUserActionManager actionManager, 
        ILaminarStorageItemFactory factory,
        IPersistentDataManager dataManager, 
        IFileSystem fileSystem)
    {
        _actionManager = actionManager;
        _factory = factory;
        _fileSystem = fileSystem;
        
        var dataStore = dataManager.GetDataStore(DataStoreKey.PersistentData).CreateChild("FileBrowser");
        
        RootFolders = dataStore
            .InitializeDefaultValue<List<FileSystemPath>>(nameof(RootFolders), [ dataManager.Path.ChildPath("Default") ])
            .ToObservableCollection()
            .ObservableMap(_factory.CreateRootFolder);
    }

    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; }

    public IUserActionResult AddDefault<T>(ILaminarStorageFolder parentFolder, IActionScope? scope = null) 
        where T : class, ILaminarStorageItem
    {
        return _actionManager.ExecuteAction(new AddDefaultStorageItemAction<T>(_fileSystem, parentFolder, _factory), scope);
    }

    public IUserActionResult Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex,
        IActionScope? scope)
    {
        return _actionManager.ExecuteAction(new MoveStorageItemAction(itemToMove, destinationFolder, _fileSystem, destinationIndex), scope);
    }

    public IUserActionResult Delete<T>(T itemToDelete, IActionScope? scope) where T : class, ILaminarStorageItem
    {
        return _actionManager.ExecuteAction(new DeleteStorageItemAction<T>(_fileSystem, itemToDelete), scope);
    }

    public IUserActionResult Rename(ILaminarStorageItem itemToRename, string newName, IActionScope? scope)
    {
        return _actionManager.ExecuteAction(new RenameStorageItemAction(newName, itemToRename, _fileSystem), scope);
    }

    public bool OpenInSystemFileBrowser(ILaminarStorageItem item)
    {
        return _fileSystem.OpenInSystemFileBrowser(item.Path);
    } 
    
    public void Dispose()
    {
        foreach (var rootFolder in RootFolders)
        {
            rootFolder.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}