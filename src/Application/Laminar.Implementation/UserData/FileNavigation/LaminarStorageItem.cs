using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public abstract partial class LaminarStorageItem<T>(T fileSystemInfo, ILogger<ILaminarStorageItem>? logger) 
    : ILaminarStorageItem where T : FileSystemInfo
{
    protected T FileSystemInfo { get; } = fileSystemInfo;

    protected ILogger<ILaminarStorageItem>? Logger { get; } = logger;

    public string Extension { get; } = fileSystemInfo.Extension;
    
    public string Path => FileSystemInfo.FullName;

    public string Name
    {
        get;
        set
        {
            if (ParentFolder is null)
            {
                Error("Cannot rename a storage item since it does not have a parent");
                return;
            }
            
            if (value == field || !TryMoveTo(System.IO.Path.Combine(ParentFolder.Path, value + Extension)))
            {
                return;
            }
            
            SetField(ref field, value);
            OnPropertyChanged(nameof(Path));
        }
    } = string.IsNullOrEmpty(fileSystemInfo.Extension) ? fileSystemInfo.Name : fileSystemInfo.Name.Replace(fileSystemInfo.Extension, string.Empty);

    public virtual bool IsEnabled
    {
        get;
        set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(IsEffectivelyEnabled));
        }
    } = true;

    public virtual bool IsEffectivelyEnabled => IsEnabled && (ParentFolder is null || ParentFolder.IsEffectivelyEnabled);

    public bool NeedsName { get; set => SetField(ref field, value); }

    public override bool Equals(object? obj)
    {
        return obj is LaminarStorageItem<T> storageItem && storageItem.Path == Path;
    }

    public override int GetHashCode()
    {
        return Path.GetHashCode();
    }

    public void Delete()
    {
        if (ParentFolder is null)
        {
            Error("Cannot delete storage item since it does not have a parent");
            return;
        }
        
        if (ParentFolder.Contents.Remove(this))
        {
            FileSystemInfo.Delete();
        }
    }

    public bool TryMoveTo(string newPath)
    {
        try
        {
            MoveTo(newPath);
            return true;
        }
        catch (IOException ioException)
        {
            if (Logger is not null) LogCannotMoveStorageItem(Logger, Path, newPath);
            ExceptionRaised?.Invoke(this, ioException);
            return false;
        }
    }
    
    protected abstract void MoveTo(string newPath);

    public abstract ILaminarStorageFolder? ParentFolder { get; }
    
    public event EventHandler<IOException>? ExceptionRaised;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<TField>(ref TField field, TField value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TField>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void Error(string message)
    {
        Logger?.LogError(message);
        ExceptionRaised?.Invoke(this, new IOException(message));
    }
    
    [LoggerMessage(LogLevel.Error, "Cannot move storage item from '{path}' to '{newPath}'")]
    static partial void LogCannotMoveStorageItem(ILogger<ILaminarStorageItem> logger, string path, string newPath);
}