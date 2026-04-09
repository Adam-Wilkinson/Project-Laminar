using System;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageRootFolder : LaminarStorageFolder, ILaminarStorageRootFolder
{
    private readonly IDisposable _monitor;

    public LaminarStorageRootFolder(
        FileSystemPath path, 
        ILaminarStorageItemFactory factory,
        IFileSystem fileSystem,
        ILaminarFileSystemMonitor monitor,
        ILogger<LaminarStorageItem> logger) : base(path, factory, fileSystem, logger)
    {
        Path = path;
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