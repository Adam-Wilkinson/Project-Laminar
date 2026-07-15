using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Graph;

namespace Laminar.Implementation.Storage.FileExplorer;

internal abstract class FileSystemItem : IFileSystemItem, IMutableFileSystemItem
{
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemGraph _graph;
    private readonly IPersistentValue<string> _name;
    private readonly IPersistentValue<bool> _isEnabled;
    
    protected FileSystemItem(IPersistentDictionary persistentData, IFileSystem fileSystem, IFileSystemGraph graph)
    {
        _fileSystem = fileSystem;
        _graph = graph;
        PersistentStorage = persistentData;

        _name = PersistentStorage[IFileSystemItemFactory.PersistenceNameKey].GetValue<string>();
        _isEnabled = PersistentStorage[nameof(IsEnabled)].GetValueOrInitialize(true); 
    }

    internal IPersistentDictionary PersistentStorage { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual FileSystemPath Path => ParentFolder is not null
        ? ComputePathFromParent(ParentFolder)
        : throw new InvalidOperationException("Non-root storage items must have a parent");

    public abstract FileSystemItemType Info { get; }

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
        get => _isEnabled.Value;
        set
        {
            if (value == _isEnabled.Value) return;
            _isEnabled.Value = value;
            OnPropertyChanged();
            OnEffectivelyEnabledChanged();
        }
    }

    public virtual bool IsEffectivelyEnabled => IsEnabled && (ParentFolder is null || ParentFolder.IsEffectivelyEnabled);
    
    public event EventHandler? Deleted;
    
    public IFileSystemFolder? ParentFolder { get; private set; }

    public void Refresh()
    {
        if (!_fileSystem.Exists(Path))
        {
            _graph.Remove(this);
            return;
        }
        
        RefreshOverride();
        OnPropertyChanged(nameof(Path));
    }
    
    public virtual void SetNameInternal(FileSystemGraph.MutationToken _, string newNameWithExtension)
    {
        if (newNameWithExtension == _name.Value) return;
        _name.Value = newNameWithExtension;
        OnPropertyChanged(nameof(Path));
    }

    public void SetParentInternal(FileSystemGraph.MutationToken _, IFileSystemFolder newParent) => SetParent(newParent);

    public void OnDeleted()
    {
        OnDeletedOverride();
        Deleted?.Invoke(this, EventArgs.Empty);
    }
    
    internal virtual void OnEffectivelyEnabledChanged()
    {
        OnPropertyChanged(nameof(IsEffectivelyEnabled));
    }
    
    protected void SetParent(IFileSystemFolder newParent)
    {
        if (ParentFolder == newParent)
            return;
        
        ParentFolder = newParent;
        OnPropertyChanged(nameof(Path));
    }
    
    protected FileSystemPath ComputePathFromParent(IFileSystemFolder parent) => parent.Path.ChildPath(_name.Value);

    protected virtual void RefreshOverride()
    {
    }

    protected virtual void OnDeletedOverride()
    {
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