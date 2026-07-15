using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer.Synchronization;

internal sealed class FileSystemMonitor(
    IFileSystemSynchronizer fileSystemSynchronizer,
    IFileSystem fileSystem,
    IDispatcher dispatcher,
    ILogger<IFileSystemMonitor> logger)
    : IFileSystemMonitor, IDisposable
{
    private static readonly TimeSpan FileSystemModifiedRefreshDelay = new(0, 0, 0, 0, 300);

    private readonly Channel<MonitorEventArgs> _updateChannel = Channel.CreateUnbounded<MonitorEventArgs>(
    new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });
    private readonly HashSet<IFileSystemRootFolder> _outdatedFolders = [];
    private readonly Lock _mutationLock = new();
    private readonly List<IDisposable> _monitors = [];

    private readonly HashSet<(WatcherChangeTypes changeType, FileSystemPath? oldPath, FileSystemPath? newPath)> _suppressedEvents = [];
    private readonly Lock _suppressedEventsLock = new();

    private Task? _processFileSystemEventsTask;
    private CancellationTokenSource? _refreshCts;

    public void SuppressNotification(WatcherChangeTypes changeType, FileSystemPath? oldPath, FileSystemPath? newPath)
    {
        lock (_suppressedEventsLock)
        {
            _suppressedEvents.Add((changeType, oldPath, newPath));
        }
    }

    public IDisposable StartMonitoring(IFileSystemRootFolder folder, FileSystemPath[]? excludedPaths = null)
    {
        _processFileSystemEventsTask ??= Task.Run(ProcessFileSystemEvents);
        excludedPaths ??= [];
        
        var folderWatcher = fileSystem.GetFileWatcher(folder.Path);
        folderWatcher.IncludeSubdirectories = true;
        folderWatcher.EnableRaisingEvents = true;

        folderWatcher.Renamed += (_, e) => OnFileSystemEvent(e, folder, excludedPaths);
        folderWatcher.Created += (_, e) => OnFileSystemEvent(e, folder, excludedPaths);
        folderWatcher.Deleted += (_, e) => OnFileSystemEvent(e, folder, excludedPaths);
        folderWatcher.Changed += (_, e) => OnFileSystemEvent(e, folder, excludedPaths);
        folderWatcher.Error += (_, e) => OnFileSystemError(e, folder);
        _monitors.Add(folderWatcher);
        return folderWatcher;
    }

    private void OnFileSystemEvent(FileSystemEventArgs e, IFileSystemRootFolder folder, FileSystemPath[] excludedPaths)
    {
        if (excludedPaths.Contains(e.FullPath))
        {
            return;
        }

        var laminarEvent = e.ChangeType switch
        {
            WatcherChangeTypes.Changed => FileSystemEvent.Changed(e.FullPath),
            WatcherChangeTypes.Created => FileSystemEvent.Created(e.FullPath),
            WatcherChangeTypes.Deleted => FileSystemEvent.Deleted(e.FullPath),
            WatcherChangeTypes.Renamed when e is RenamedEventArgs renamed => 
                FileSystemEvent.Renamed(renamed.OldFullPath, renamed.FullPath),
            var unknown => 
                throw new InvalidOperationException($"Cannot make match for event type {unknown}")
        };
        
        _updateChannel.Writer.TryWrite(new MonitorEventArgs(folder, laminarEvent));
    }

    private void OnFileSystemError(ErrorEventArgs e, IFileSystemRootFolder folder)
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

    private void HandleFileSystemEvent(MonitorEventArgs monitorEventArgs)
    {
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();
        
        lock (_mutationLock)
        {
            _outdatedFolders.Add(monitorEventArgs.RootFolder); 
            fileSystemSynchronizer.OnFileSystemEvent(monitorEventArgs.FileSystemEvent);
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
                logger.LogTrace("Triggering file system refresh after detected change");

                List<IFileSystemRootFolder> snapshot;
                lock (_mutationLock)
                {
                    snapshot = [.. _outdatedFolders];
                    _outdatedFolders.Clear();
                }

                fileSystemSynchronizer.ReconcileAndReset(snapshot);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                await dispatcher.InvokeAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
            }
        }, token);
    }
    
    public void Dispose()
    {
        _refreshCts?.Dispose();
        _processFileSystemEventsTask?.Dispose();
        foreach (var monitor in _monitors)
        {
            monitor.Dispose();
        }
    }

    private record struct MonitorEventArgs(IFileSystemRootFolder RootFolder, FileSystemEvent FileSystemEvent);
}
