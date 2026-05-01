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

    protected ILogger<LaminarStorageItem> Logger { get; }

    internal IPersistentDictionary PersistentStorage { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual FileSystemPath Path => ParentFolder?.Path.ChildPath(_nameWithExtension) 
                                          ?? throw new InvalidOperationException("Non-root storage items must have a parent");

    public string UserFriendlyName
    {
        get
        {
            var possible = _fileSystem.GetNameWithoutExtension(Path);
            return string.IsNullOrWhiteSpace(possible) ? Path.NameAndExtension : possible;
        }
    }

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
    
    public event EventHandler? RootFolderDisposed;

    protected void OnParentRootFolderDisposed(object? sender, EventArgs e) => RootFolderDisposed?.Invoke(sender, e);

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

        (ParentFolder as LaminarStorageItem)?.RootFolderDisposed -= OnParentRootFolderDisposed;

        if (folder is not null && ParentFolder is not null && _fileSystem.Exists(Path))
        {
            var destinationPath = folder.Path.ChildPath(Path.NameAndExtension);
            _fileSystem.Move(Path, destinationPath);
        }
        
        ParentFolder = folder;
        (ParentFolder as LaminarStorageItem)?.RootFolderDisposed += OnParentRootFolderDisposed;
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