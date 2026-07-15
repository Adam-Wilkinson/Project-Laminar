using System.ComponentModel;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer;

/// <summary>
/// A representation of a storage item on the file system that is read-only,
/// which is modified by the <see cref="IFileBrowser"/>
/// </summary>
public interface IFileSystemItem : INotifyPropertyChanged
{
    public FileSystemPath Path { get; }

    public FileSystemItemType Info { get; }
    
    public string UserFriendlyName { get; }

    public bool IsEnabled { get; set; }
    
    public bool IsEffectivelyEnabled { get; }
    
    public IFileSystemFolder? ParentFolder { get; }

    public event EventHandler? Deleted;
    
    public void Refresh();
}