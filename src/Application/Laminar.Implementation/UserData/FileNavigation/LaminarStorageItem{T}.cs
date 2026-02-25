using System;
using System.IO;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public abstract partial class LaminarStorageItem<T> : LaminarStorageItem where T : FileSystemInfo
{
    protected LaminarStorageItem(T fileSystemInfo, IFileSystem fileSystem, ILogger<LaminarStorageItem>? logger)
        : base(logger)
    {
        FileSystemInfo = fileSystemInfo;
        Extension = fileSystemInfo.Extension;
        Name = string.IsNullOrEmpty(fileSystemInfo.Extension)
            ? fileSystemInfo.Name
            : fileSystemInfo.Name.Replace(fileSystemInfo.Extension, string.Empty);
    }

    public override T FileSystemInfo { get; }
    
    public override string Path => FileSystemInfo.FullName;

    public override bool Equals(object? obj)
    {
        return obj is LaminarStorageItem<T> storageItem && storageItem.Path == Path;
    }

    public override int GetHashCode()
    {
        return Path.GetHashCode();
    }

    internal bool TryMoveTo(string newPath)
    {
        try
        {
            MoveTo(newPath);
            return true;
        }
        catch (IOException ioException)
        {
            if (Logger is not null) LogCannotMoveStorageItem(Logger, Path, newPath, ioException);
            OnExceptionRaised(ioException);
            return false;
        }
    }
    
    protected abstract void MoveTo(string newPath);
    
    public override void Refresh()
    {
        FileSystemInfo.Refresh();
    }
    
    [LoggerMessage(LogLevel.Error, "Cannot move storage item from '{path}' to '{newPath}'")]
    static partial void LogCannotMoveStorageItem(ILogger<ILaminarStorageItem> logger, string path, string newPath, Exception exception);
}