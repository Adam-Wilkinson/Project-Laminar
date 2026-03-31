using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public abstract class LaminarStorageItem(ILogger<LaminarStorageItem>? logger) : ILaminarStorageItem
{
    protected static void SetParent(LaminarStorageItem item, ILaminarStorageFolder? folder) => item.ParentFolder = folder;
    
    protected static void TriggerOnDeleted(LaminarStorageItem item)
    {
        item.OnDeleted?.Invoke(item, EventArgs.Empty);
        SetParent(item, null);
    }
    
    protected ILogger<LaminarStorageItem>? Logger { get; } = logger;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? OnDeleted;
    
    public string Name
    {
        get;
        internal set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(Path));
        }
    } = "";

    public virtual string Path => ParentFolder is not null
        ? System.IO.Path.Combine(ParentFolder.Path, Name + Extension)
        : Name + Extension;
    
    public abstract IObservableValue<long> SizeOnDisk { get; }

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
    
    public ILaminarStorageFolder? ParentFolder { get; private set; }
    
    public abstract void Refresh();
    
    public virtual void OnEffectivelyEnabledChanged()
    {
        OnPropertyChanged(nameof(IsEffectivelyEnabled));
    }
    
    public override bool Equals(object? obj)
    {
        return obj is LaminarStorageItem storageItem && storageItem.Path == Path;
    }

    public override int GetHashCode()
    {
        return Path.GetHashCode();
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