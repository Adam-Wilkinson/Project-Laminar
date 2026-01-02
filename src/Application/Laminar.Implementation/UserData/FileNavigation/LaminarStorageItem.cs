using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public abstract partial class LaminarStorageItem<T>(T fileSystemInfo, ILogger<ILaminarStorageItem>? logger) : ILaminarStorageItem where T : FileSystemInfo
{
    protected T FileSystemInfo { get; } = fileSystemInfo;

    protected ILogger<ILaminarStorageItem>? Logger { get; } = logger;

    public bool ParentIsEffectivelyEnabled
    {
        get;
        set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(IsEffectivelyEnabled));
        }
    }

    public string Extension { get; } = fileSystemInfo.Extension;
    
    public string Path => FileSystemInfo.FullName;

    public string Name
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            MoveTo(System.IO.Path.Combine(ParentFolder.Path, value + Extension));
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
    }

    public bool IsEffectivelyEnabled => IsEnabled && ParentIsEffectivelyEnabled;

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
        if (ParentFolder.Contents.Remove(this))
        {
            FileSystemInfo.Delete();
        }
    }

    public void MoveTo(string newPath)
    {
        try
        {
            TryMoveTo(newPath);
        }
        catch (IOException ioException)
        {
            OnPropertyChanged(nameof(Name));
            if (Logger is not null) LogCannotMoveStorageItem(Logger, Path, newPath);
            ExceptionRaised?.Invoke(this, ioException);
        }
    }
    
    protected abstract void TryMoveTo(string newPath);

    public abstract ILaminarStorageFolder ParentFolder { get; }
    
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

    [LoggerMessage(LogLevel.Error, "Cannot move storage item from {path} to {newPath}")]
    static partial void LogCannotMoveStorageItem(ILogger<ILaminarStorageItem> logger, string path, string newPath);
}