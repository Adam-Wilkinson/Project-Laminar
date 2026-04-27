using System;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class LaminarStorageRootFolder : LaminarStorageFolder, ILaminarStorageRootFolder
{
    private const string InfoFileName = ".project-laminar-data";
    private readonly IFileSystem _fileSystem;
    private readonly ILaminarFileSystemMonitor _fileSystemMonitor;
    private readonly IPersistentDataManager _persistentDataManager;
    
    private FileSystemPath _path;
    private IDisposable _currentMonitor;
    
    public LaminarStorageRootFolder(
        FileSystemPath path, 
        ILaminarStorageItemFactory factory,
        IFileSystem fileSystem,
        IPersistentDataManager persistentDataManager,
        ILaminarFileSystemMonitor monitor,
        ILogger<LaminarStorageItem> logger) 
        : base(path, factory, fileSystem, 
            persistentDataManager.GetDataStore(new DataStoreKey(InfoFileName, PersistentDataType.Json, path)), persistentDataManager, logger)
    {
        _path = path;
        _fileSystem = fileSystem;
        _fileSystemMonitor = monitor;
        _persistentDataManager = persistentDataManager;
        // TODO: .json should not be hard-coded
        _currentMonitor = monitor.StartMonitoring(this, [ path.ChildPath(InfoFileName + ".json") ]);
        Refresh();
    }

    public override FileSystemPath Path => _path;

    public override void Rename(string newNameWithExtension)
    {
        if (Path.NameAndExtension == newNameWithExtension || ParentFolder is null) return;
        
        if (Path.Parent is not { } parentPath) 
            throw new InvalidOperationException();

        var oldPath = Path;
        var newPath = parentPath.ChildPath(newNameWithExtension);
        
        _fileSystem.Move(oldPath, newPath);
        _path = newPath;
        OnPropertyChanged(nameof(Path));
        PersistentStorage.SetValue(nameof(NameKey), newNameWithExtension);
        
        _currentMonitor.Dispose();
        _currentMonitor = _fileSystemMonitor.StartMonitoring(this);
    }
    
    public void Dispose()
    {
        _currentMonitor.Dispose();
        _persistentDataManager.ForgetDataStore(new DataStoreKey(InfoFileName, PersistentDataType.Json, Path));
        OnParentRootFolderDisposed(this, EventArgs.Empty);
        GC.SuppressFinalize(this);
    }
}