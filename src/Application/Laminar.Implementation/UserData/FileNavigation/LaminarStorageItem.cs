using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public abstract class LaminarStorageItem(ILogger<LaminarStorageItem>? logger) : ILaminarStorageItem
{
    protected ILogger<LaminarStorageItem>? Logger { get; } = logger;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get;
        internal set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(Path));
        }
    } = "";
    
    public abstract string Path { get; }

    public abstract FileSystemInfo FileSystemInfo { get; }

    public string Extension { get; protected init; } = "";
    
    public virtual bool IsEnabled
    {
        get;
        set
        {
            SetField(ref field, value);
            OnEffectivelyEnabledChanged();
        }
    } = true;
    
    public virtual bool IsEffectivelyEnabled => IsEnabled && (ParentFolder is null || ParentFolder.IsEffectivelyEnabled);
    
    public bool NeedsName { get; set => SetField(ref field, value); }
    
    public ILaminarStorageFolder? ParentFolder { get; set; }

    public event EventHandler<IOException>? ExceptionRaised;
    
    public abstract void Refresh();

    public virtual void OnEffectivelyEnabledChanged()
    {
        OnPropertyChanged(nameof(IsEffectivelyEnabled));
    }
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void OnExceptionRaised(IOException exception)
    {
        ExceptionRaised?.Invoke(this, exception);
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
        OnExceptionRaised(new IOException(message));
    }
}