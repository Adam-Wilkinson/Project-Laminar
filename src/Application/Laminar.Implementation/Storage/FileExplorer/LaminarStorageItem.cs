using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal abstract class LaminarStorageItem(
    IFileSystem fileSystem,
    ILogger<LaminarStorageItem> logger)
    : ILaminarStorageItem
{
    private string _nameWithExtension = "";

    protected ILogger<LaminarStorageItem> Logger { get; } = logger;

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual FileSystemPath Path => ParentFolder?.Path.ChildPath(_nameWithExtension) 
                                          ?? throw new InvalidOperationException("Non-root storage items must have a parent");

    public virtual bool IsEnabled
    {
        get;
        set
        {
            if (!SetField(ref field, value)) return;
            OnEffectivelyEnabledChanged();
        }
    } = true;
    
    public virtual bool IsEffectivelyEnabled => IsEnabled && (ParentFolder is null || ParentFolder.IsEffectivelyEnabled);
    
    public bool NeedsName { get; set => SetField(ref field, value); }
    
    public ILaminarStorageFolder? ParentFolder { get; private set; }

    public void Refresh()
    {
        if (!fileSystem.Exists(Path)) return;
        
        RefreshOverride();
        OnPropertyChanged(nameof(Path));
    }

    public void Rename(string newNameWithExtension)
    {
        ArgumentNullException.ThrowIfNull(ParentFolder);
        if (!string.IsNullOrWhiteSpace(_nameWithExtension) && fileSystem.Exists(Path)) 
        {
            fileSystem.Move(Path, ParentFolder.Path.ChildPath(newNameWithExtension));
        }

        _nameWithExtension = newNameWithExtension;
        OnPropertyChanged(nameof(Path));
    }

    public void SetParent(ILaminarStorageFolder? folder)
    {
        if (ParentFolder == folder)
        {
            return;
        }

        if (ParentFolder?.Contents is IObservableCollection<ILaminarStorageItem> oldParentContents)
        {
            oldParentContents.Remove(this);
        }

        if (folder is not null && ParentFolder is not null && fileSystem.Exists(Path))
        {
            var destinationPath = folder.Path.ChildPath(Path.NameAndExtension);
            fileSystem.Move(Path, destinationPath);
        }
        
        ParentFolder = folder;
        OnPropertyChanged(nameof(Path));
    }
    
    public virtual void OnEffectivelyEnabledChanged()
    {
        OnPropertyChanged(nameof(IsEffectivelyEnabled));
    }

    protected abstract void RefreshOverride();

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
}