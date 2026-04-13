using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Laminar.Implementation.UserData.FileNavigation;

internal class LaminarFileSystemMonitor : ILaminarFileSystemMonitor
{
    private static readonly TimeSpan FileSystemModifiedRefreshDelay = new(0, 0, 0, 0, 300);
    private readonly IDeletedStorageItemCache _deletedStorageItemCache;
    private readonly IFileSystem _fileSystem;
    private readonly ILaminarStorageItemFactory _factory;
    private readonly ILogger<ILaminarFileSystemMonitor> _logger;
    private readonly Channel<LaminarFileSystemEvent> _updateChannel = Channel.CreateUnbounded<LaminarFileSystemEvent>(
    new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });
    private readonly HashSet<ILaminarStorageRootFolder> _outdatedFolders = [];

    private CancellationTokenSource? _refreshCts;

    public LaminarFileSystemMonitor(
        IDeletedStorageItemCache deletedItemCache, 
        IFileSystem fileSystem, 
        ILaminarStorageItemFactory factory,
        ILogger<ILaminarFileSystemMonitor> logger)
    {
        _deletedStorageItemCache = deletedItemCache;
        _fileSystem = fileSystem;
        _logger = logger;
        _factory = factory;

        Task.Run(ProcessFileSystemEvents);
    }

    public IDisposable StartMonitoring(ILaminarStorageRootFolder folder)
    {
        var folderWatcher = _fileSystem.CreateFileWatcher(folder.Path);
        folderWatcher.IncludeSubdirectories = true;
        folderWatcher.EnableRaisingEvents = true;

        folderWatcher.Renamed += (_, e) => OnFileSystemEvent(e, folder);
        folderWatcher.Created += (_, e) => OnFileSystemEvent(e, folder);
        folderWatcher.Deleted += (_, e) => OnFileSystemEvent(e, folder);
        folderWatcher.Changed += (_, e) => OnFileSystemEvent(e, folder);
        folderWatcher.Error += (_, e) => OnFileSystemError(e, folder);
        return folderWatcher;
    }

    private void OnFileSystemEvent(FileSystemEventArgs e, ILaminarStorageRootFolder folder)
    {
        _updateChannel.Writer.TryWrite(new LaminarFileSystemEvent(e, folder));
    }

    private void OnFileSystemError(ErrorEventArgs e, ILaminarStorageRootFolder folder)
    {
        _logger.LogError(e.GetException(), "Error when processing file system changed events. Refreshing manually");
        folder.Refresh();
    }

    private async Task ProcessFileSystemEvents()
    {
        try
        {
            await foreach (var item in _updateChannel.Reader.ReadAllAsync())
            {
                try
                {
                    HandleFileSystemEvent(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error when processing file system changed events. Refreshing manually");
                    ScheduleRefresh();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error when processing file system changed events");
            throw;
        }
    }

    private void HandleFileSystemEvent(LaminarFileSystemEvent laminarFileSystemEvent)
    {
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();

        _outdatedFolders.Add(laminarFileSystemEvent.OriginatingFolder);
        var e = laminarFileSystemEvent.EventArgs;
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Renamed:
                if (e is not RenamedEventArgs renamedEventArgs || Path.GetFileName(renamedEventArgs.Name) is not string newName || _factory.TryGetExisting(renamedEventArgs.OldFullPath) is not LaminarStorageItem renamedItem)
                {
                    _logger.LogWarning("Could not find renamed item {oldName}, this may result in a rename operation being considered as separate delete and create operations", e.Name);
                    break;
                }
                renamedItem.Rename(newName);
                break;
            case WatcherChangeTypes.Deleted:
                if (_factory.TryGetExisting(e.FullPath) is not ILaminarStorageItem deletedItem)
                {
                    _logger.LogWarning("Could not find deleted item {itemName}, this may result in a move operation being missed", e.Name);
                    break;
                }
                _deletedStorageItemCache.RegisterPotentialDeletion(deletedItem);
                break;
        }

        ScheduleRefresh();
    }

    private void ScheduleRefresh()
    {
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();

        var token = _refreshCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(FileSystemModifiedRefreshDelay, token);
                _logger.LogTrace("Triggering file system refresh after scheduled move operations");
                foreach (var folder in _outdatedFolders)
                {
                    folder.Refresh();
                }
                _outdatedFolders.Clear();
                _deletedStorageItemCache.Clear();
            }
            catch (TaskCanceledException) { }
        }, token);
    }

    private record struct LaminarFileSystemEvent(FileSystemEventArgs EventArgs, ILaminarStorageRootFolder OriginatingFolder);
}
