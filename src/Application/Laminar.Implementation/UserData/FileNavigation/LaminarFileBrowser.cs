using System;
using System.Collections.Generic;
using System.IO;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Notification;
using Laminar.Implementation.UserData.FileNavigation.UserActions;

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
        
        dataStore.InitializeDefaultValue<List<string>>(nameof(RootFolders), [
            Path.Combine(dataManager.Path, "Default")
        ]);
        
        var rootFolderPaths = 
            new SourcedObservableCollection<string>(dataStore.GetItem<List<string>>(nameof(RootFolders)).Result!);
        
        RootFolders = new MappedObservableCollection<string, ILaminarStorageRootFolder>(rootFolderPaths, path =>
            _factory.FromPath<ILaminarStorageRootFolder>(path));
        
        dataStore.GetObservable<List<string>>(nameof(RootFolders)).ValueChanged += (o, e) =>
        {
            rootFolderPaths.ChangeSourceTo(e.NewValue);
        };
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