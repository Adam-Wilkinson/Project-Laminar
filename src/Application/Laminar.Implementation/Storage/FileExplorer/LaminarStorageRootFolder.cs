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
    private readonly IDisposable _monitor;
    private readonly IPersistentDataNode _dataStore;
    
    
    public LaminarStorageRootFolder(
        FileSystemPath path, 
        ILaminarStorageItemFactory factory,
        IFileSystem fileSystem,
        IPersistentDataManager persistentDataManager,
        ILaminarFileSystemMonitor monitor,
        ILogger<LaminarStorageItem> logger) : base(path, factory, fileSystem, logger)
    {
        Path = path;
        _dataStore = persistentDataManager.GetDataStore(new DataStoreKey(InfoFileName, PersistentDataType.Json, path));

        _dataStore.InitializeDefaultValue("RootFolder", this, (this, factory));
        
        _monitor = monitor.StartMonitoring(this);
        Refresh();
    }

    public override FileSystemPath Path { get; }

    public void Dispose()
    {
        _monitor.Dispose();
        GC.SuppressFinalize(this);
    }
}