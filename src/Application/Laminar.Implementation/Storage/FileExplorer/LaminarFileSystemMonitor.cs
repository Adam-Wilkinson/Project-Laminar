using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal partial class LaminarFileSystemMonitor(
    IDeletedStorageItemCache deletedItemCache,
    IFileSystem fileSystem,
    ILaminarStorageItemFactory factory,
    IDispatcher dispatcher,
    ILogger<ILaminarFileSystemMonitor> logger)
    : ILaminarFileSystemMonitor, IDisposable
{
    private static readonly TimeSpan FileSystemModifiedRefreshDelay = new(0, 0, 0, 0, 300);

    private readonly Channel<LaminarFileSystemEvent> _updateChannel = Channel.CreateUnbounded<LaminarFileSystemEvent>(
    new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });
    private readonly HashSet<ILaminarStorageRootFolder> _outdatedFolders = [];
    private readonly Lock _outdatedFoldersLock = new();
    private readonly List<IDisposable> _monitors = [];

    private Task? _processFileSystemEventsTask;
    private CancellationTokenSource? _refreshCts;

    public IDisposable StartMonitoring(ILaminarStorageRootFolder folder)
    {
        _processFileSystemEventsTask ??= Task.Run(ProcessFileSystemEvents);
        
        var folderWatcher = fileSystem.CreateFileWatcher(folder.Path);
        folderWatcher.IncludeSubdirectories = true;
        folderWatcher.EnableRaisingEvents = true;

        folderWatcher.Renamed += (_, e) => OnFileSystemEvent(e, folder);
        folderWatcher.Created += (_, e) => OnFileSystemEvent(e, folder);
        folderWatcher.Deleted += (_, e) => OnFileSystemEvent(e, folder);
        folderWatcher.Changed += (_, e) => OnFileSystemEvent(e, folder);
        folderWatcher.Error += (_, e) => OnFileSystemError(e, folder);
        _monitors.Add(folderWatcher);
        return folderWatcher;
    }

    private void OnFileSystemEvent(FileSystemEventArgs e, ILaminarStorageRootFolder folder)
    {
        _updateChannel.Writer.TryWrite(new LaminarFileSystemEvent(e, folder));
    }

    private void OnFileSystemError(ErrorEventArgs e, ILaminarStorageRootFolder folder)
    {
        logger.LogError(e.GetException(), "Error when processing file system changed events. Refreshing manually");
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
                    logger.LogError(ex, "Error when processing file system changed events. Refreshing manually");
                    ScheduleRefresh();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error when processing file system changed events");
            await dispatcher.InvokeAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
        }
    }

    private void HandleFileSystemEvent(LaminarFileSystemEvent laminarFileSystemEvent)
    {
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();
        
        lock (_outdatedFoldersLock)
        {
            _outdatedFolders.Add(laminarFileSystemEvent.OriginatingFolder);
        }

        FileSystemEventArgs e = laminarFileSystemEvent.EventArgs;
        if (e.Name is null) return;
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Renamed:
                if (e is not RenamedEventArgs renamedEventArgs 
                    || Path.GetFileName(renamedEventArgs.Name) is not { } newName 
                    || factory.TryGetExisting(renamedEventArgs.OldFullPath) is not LaminarStorageItem renamedItem)
                {
                    LogCouldNotFindRenamedItem(e.Name);
                    break;
                }
                renamedItem.Rename(newName);
                break;
            case WatcherChangeTypes.Deleted:
                if (factory.TryGetExisting(e.FullPath) is not { } deletedItem)
                {
                    LogCouldNotFindDeletedItem(e.Name);
                    break;
                }
                deletedItemCache.RegisterPotentialDeletion(deletedItem);
                break;
            case WatcherChangeTypes.Created:
            case WatcherChangeTypes.Changed:
            case WatcherChangeTypes.All:
            default:
                break;
        }

        ScheduleRefresh();
    }

    private void ScheduleRefresh()
    {
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();

        CancellationToken token = _refreshCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(FileSystemModifiedRefreshDelay, token);
                logger.LogTrace("Triggering file system refresh after scheduled move operations");

                List<ILaminarStorageRootFolder> snapshot;
                lock (_outdatedFoldersLock)
                {
                    snapshot = [.. _outdatedFolders];
                    _outdatedFolders.Clear();
                }

                foreach (var folder in snapshot)
                {
                    folder.Refresh();
                }
                deletedItemCache.Clear();
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                await dispatcher.InvokeAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
            }
        }, token);
    }

    private record struct LaminarFileSystemEvent(FileSystemEventArgs EventArgs, ILaminarStorageRootFolder OriginatingFolder);

    public void Dispose()
    {
        _refreshCts?.Dispose();
        _processFileSystemEventsTask?.Dispose();
        foreach (var monitor in _monitors)
        {
            monitor.Dispose();
        }
        
        GC.SuppressFinalize(this);
    }

    [LoggerMessage(LogLevel.Warning, "Could not find renamed item {oldName}, this may result in a rename operation being considered as separate delete and create operations")]
    partial void LogCouldNotFindRenamedItem(string oldName);

    [LoggerMessage(LogLevel.Warning, "Could not find deleted item {itemName}, this may result in a move operation being missed")]
    partial void LogCouldNotFindDeletedItem(string itemName);
}
