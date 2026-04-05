using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public abstract class LaminarStorageItem(IFileSystem fileSystem, ILogger<LaminarStorageItem> logger) : ILaminarStorageItem
{
    private string _nameWithExtension = "";
    
    protected static void SetParent(LaminarStorageItem item, ILaminarStorageFolder? folder)
    { 
        item.ParentFolder = folder;
        item.OnPropertyChanged(nameof(item.Path));
    }

    protected static void Rename(LaminarStorageItem item, string newNameWithExtension)
    {
        item._nameWithExtension = newNameWithExtension;
        item.OnPropertyChanged(nameof(item.Path));
    }
    
    protected ILogger<LaminarStorageItem> Logger { get; } = logger;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual FileSystemPath Path => ParentFolder?.Path.ChildPath(_nameWithExtension) 
                                          ?? throw new InvalidOperationException("Non-root storage items must have a parent");

    public abstract IObservableValue<long> SizeOnDisk { get; }
    
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
    
    public ILaminarStorageFolder? ParentFolder { get; private set; }

    public void Refresh()
    {
        if (fileSystem.Exists(Path))
        {
            RefreshOverride();
        }
    }
    
    protected abstract void RefreshOverride();
    
    public virtual void OnEffectivelyEnabledChanged()
    {
        OnPropertyChanged(nameof(IsEffectivelyEnabled));
    }
    
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