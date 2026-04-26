using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal abstract class LaminarStorageItem : ILaminarStorageItem
{
    public const string NameKey = "Name";
    private readonly IFileSystem _fileSystem;
    private string _nameWithExtension;
    private bool _isEnabled;

    protected LaminarStorageItem(IFileSystem fileSystem,
        ILogger<LaminarStorageItem> logger,
        IPersistentDictionary persistentData,
        string? nameWithExtension = null)
    {
        _fileSystem = fileSystem;
        PersistentStorage = persistentData;
        Logger = logger;
        if (!PersistentStorage.ContainsKey(NameKey))
        {
            ArgumentNullException.ThrowIfNull(nameWithExtension);
            PersistentStorage[NameKey].SetDefaultAndGet(nameWithExtension);
        }

        _nameWithExtension = PersistentStorage[NameKey].GetValue<string>().Value;
        _isEnabled = PersistentStorage[nameof(IsEnabled)].SetDefaultAndGet(true).Value;
    }
    
    protected LaminarStorageItem(
        IFileSystem fileSystem, 
        IPersistentDataManager persistentDataManager, 
        ILogger<LaminarStorageItem> logger,
        string nameWithExtension)
    {
        _fileSystem = fileSystem;
        PersistentStorage = persistentDataManager.GetHeadlessNode<IPersistentDictionary>();
        Logger = logger;
        _nameWithExtension = nameWithExtension;
    }

    protected ILogger<LaminarStorageItem> Logger { get; }

    internal IPersistentDictionary PersistentStorage { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual FileSystemPath Path => ParentFolder?.Path.ChildPath(_nameWithExtension) 
                                          ?? throw new InvalidOperationException("Non-root storage items must have a parent");

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (!SetField(ref _isEnabled, value)) return;
            PersistentStorage.SetValue(nameof(IsEnabled), value);
            OnEffectivelyEnabledChanged();
        }
    }

    public virtual bool IsEffectivelyEnabled => IsEnabled && (ParentFolder is null || ParentFolder.IsEffectivelyEnabled);
    
    public bool NeedsName { get; set => SetField(ref field, value); }
    
    public ILaminarStorageFolder? ParentFolder { get; private set; }

    public void Refresh()
    {
        if (!_fileSystem.Exists(Path)) return;
        
        RefreshOverride();
        OnPropertyChanged(nameof(Path));
    }

    public virtual void Rename(string newNameWithExtension)
    {
        if (_nameWithExtension == newNameWithExtension) return;
        ArgumentNullException.ThrowIfNull(ParentFolder);
        if (!string.IsNullOrWhiteSpace(_nameWithExtension) && _fileSystem.Exists(Path)) 
        {
            _fileSystem.Move(Path, ParentFolder.Path.ChildPath(newNameWithExtension));
        }

        _nameWithExtension = newNameWithExtension;
        PersistentStorage.SetValue(nameof(NameKey), _nameWithExtension);
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

        if (folder is not null && ParentFolder is not null && _fileSystem.Exists(Path))
        {
            var destinationPath = folder.Path.ChildPath(Path.NameAndExtension);
            _fileSystem.Move(Path, destinationPath);
        }
        
        ParentFolder = folder;
        OnParentChanged();
        OnPropertyChanged(nameof(Path));
    }
    
    public virtual void OnEffectivelyEnabledChanged()
    {
        OnPropertyChanged(nameof(IsEffectivelyEnabled));
    }

    protected virtual void OnParentChanged()
    {
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
    
    protected virtual void OnPersistentStorageChanged(IPersistentDictionary? oldDictionary, IPersistentDictionary newDictionary)
    {
        oldDictionary?.TryGetValue<bool>(nameof(IsEnabled))?.OnChanged -= OnPersistentIsEnabledChanged;
        oldDictionary?.TryGetValue<string>(NameKey)?.OnChanged -= OnPersistentNameChanged;
        
        var newPersistentIsEnabled = newDictionary[nameof(IsEnabled)].SetDefaultAndGet(IsEnabled);
        newPersistentIsEnabled.OnChanged += OnPersistentIsEnabledChanged;
        IsEnabled = newPersistentIsEnabled.Value;
        
        var newPersistentName = newDictionary[NameKey].SetDefaultAndGet(_nameWithExtension);
        newPersistentName.OnChanged += OnPersistentNameChanged;
        Rename(newPersistentName.Value);
    }

    private void OnPersistentNameChanged(object? sender, ObservableValueChangedEventArgs<string> e)
    {
        Rename(e.NewValue);
    }

    private void OnPersistentIsEnabledChanged(object? sender, ObservableValueChangedEventArgs<bool> e)
    {
        IsEnabled = e.NewValue;
    }
}