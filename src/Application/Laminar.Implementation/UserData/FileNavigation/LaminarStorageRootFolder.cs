using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageRootFolder : LaminarStorageFolder, ILaminarStorageRootFolder
{
    private static readonly TimeSpan FileSystemModifiedRefreshDelay = new(0, 0, 0, 0, 300);
    private readonly IFileWatcher _folderWatcher;
    private readonly Channel<FileSystemEventArgs> _fileWatcherUpdateChannel = Channel.CreateUnbounded<FileSystemEventArgs>(
        new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });
    
    private CancellationTokenSource? _refreshCts;

    public LaminarStorageRootFolder(
        FileSystemPath path, 
        ILaminarStorageItemFactory factory,
        IFileSystem fileSystem,
        ILogger<LaminarStorageItem> logger) : base(path, factory, fileSystem, logger)
    {
        Path = path;
        Refresh();

        _folderWatcher = fileSystem.CreateFileWatcher(path);
        _folderWatcher.IncludeSubdirectories = true;
        _folderWatcher.EnableRaisingEvents = true;

        _folderWatcher.Renamed += OnFileSystemEvent;
        _folderWatcher.Created += OnFileSystemEvent;
        _folderWatcher.Deleted += OnFileSystemEvent;
        _folderWatcher.Changed += OnFileSystemEvent;
        _folderWatcher.Error += OnFileSystemError;

        Task.Run(ProcessFileSystemEvents);
    }

    public override FileSystemPath? Path { get; }

    private void OnFileSystemEvent(object? sender, FileSystemEventArgs e)
    {
        _fileWatcherUpdateChannel.Writer.TryWrite(e);
    }
    
    private void OnFileSystemError(object sender, ErrorEventArgs e)
    {
        Logger.LogError(e.GetException(), "Error when processing file system changed events. Refreshing manually");
        Refresh();
    }
    
    private async Task ProcessFileSystemEvents()
    {
        try
        {
            await foreach (var item in _fileWatcherUpdateChannel.Reader.ReadAllAsync())
            {
                try
                {
                    HandleFileSystemEvent(item);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error when processing file system changed events. Refreshing manually");
                    ScheduleRefresh();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error when processing file system changed events");
            throw;
        }
    }

    private void HandleFileSystemEvent(FileSystemEventArgs e)
    {
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();

        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Renamed:
                if (e is not RenamedEventArgs renamedEventArgs ||
                    FindChildFromPath(renamedEventArgs.OldFullPath) is not LaminarStorageItem renamedChild)
                {
                    Logger.LogWarning("Could not find renamed item {oldName}, this may result in a rename operation being considered as separate delete and create operations", e.Name);
                    break;
                }
                Rename(renamedChild, e.Name!);
                break;
            case WatcherChangeTypes.Deleted:
                if (FindChildFromPath(e.FullPath) is not LaminarStorageItem deletedItem)
                {
                    Logger.LogWarning("Could not find deleted item {itemName}, this may result in a move operation being missed", e.Name);
                    break;
                }
                TriggerOnDeleted(deletedItem);
                break;
        }
        
        ScheduleRefresh();
    }

    private ILaminarStorageItem? FindChildFromPath(string absolutePath)
    {
        if (!Path.HasValue) return null;
        string relativePath = System.IO.Path.GetRelativePath(Path.Value.ToString(), absolutePath);
        ILaminarStorageFolder currentFolder = this;
        ILaminarStorageItem? currentResult = null;
        foreach (string name in relativePath.Split('/', '\\'))
        {
            currentResult = FindDirectChildNamed(currentFolder, name);

            if (currentResult is null) return null;

            if (currentResult is ILaminarStorageFolder folder) currentFolder = folder;
        }

        return currentResult;
    }
        
    private static ILaminarStorageItem? FindDirectChildNamed(ILaminarStorageFolder folder, string itemName)
    {
        Span<ILaminarStorageItem> contents = folder.Contents.ToArray();
        foreach (var item in contents)
        {
            if (item.Path?.NameAndExtension == itemName)
            {
                return item;
            }
        }

        return null;
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
                Logger.LogTrace("Triggering refresh after scheduled move operations");
                Refresh();
            }
            catch (TaskCanceledException) { }
        }, token);
    }    

    public void Dispose()
    {
        _folderWatcher.Dispose();
        GC.SuppressFinalize(this);
    }
}