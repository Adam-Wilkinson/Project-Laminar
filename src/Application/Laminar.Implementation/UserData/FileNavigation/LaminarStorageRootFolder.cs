using System;
using System.Collections.Generic;
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
    private static readonly TimeSpan FileSystemModifiedRefreshDelay = new(0, 0, 0, 0, 100);
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

    public override FileSystemPath Path { get; }

    private void OnFileSystemEvent(object? sender, FileSystemEventArgs e)
    {
        Logger.LogTrace("File system event of type {type} happened", e.ChangeType);
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
        Logger.LogTrace("START processing FS {type} event for file '{path}'", e.ChangeType, e.FullPath);
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();
        
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Renamed:
                if (e is not RenamedEventArgs renamedEventArgs || 
                    FindChildrenFromPath(renamedEventArgs.OldFullPath).LastOrDefault() is not LaminarStorageItem renamedChild) return;
                Rename(renamedChild, e.Name!);
                break;
            case WatcherChangeTypes.Deleted:
                Logger.LogTrace("Starting a deleted thing");
                if (FindChildrenFromPath(e.FullPath).LastOrDefault() is LaminarStorageItem item)
                {
                    TriggerOnDeleted(item);
                }
                break;
        }
        
        ScheduleRefresh();
        
        Logger.LogTrace("END processing FS {type} event for file '{path}'", e.ChangeType, e.FullPath);
    }

    private IEnumerable<ILaminarStorageItem> FindChildrenFromPath(string absolutePath)
    {
        string relativePath = System.IO.Path.GetRelativePath(Path.ToString(), absolutePath);
        yield return this;
        var childNames = relativePath.Split('/');
        int indexInChildNames = 0;
        ILaminarStorageFolder currentFolder = this;
        while (currentFolder.Path.ToString() != absolutePath)
        {
            switch (FindDirectChildNamed(currentFolder, childNames[indexInChildNames]))
            {
                case { } item when item.Path.ToString() == absolutePath:
                    yield return item;
                    yield break;
                case ILaminarStorageFolder childFolder:
                    indexInChildNames++;
                    currentFolder = childFolder;
                    yield return currentFolder;
                    break;
                case null:
                    throw new InvalidOperationException();
                default:
                    continue;
            }
        }
    }
        
    private ILaminarStorageItem? FindDirectChildNamed(ILaminarStorageFolder folder, string itemName)
    {
        Span<ILaminarStorageItem> contents = folder.Contents.ToArray();
        foreach (var item in contents)
        {
            if (item.Path.NameAndExtension == itemName)
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