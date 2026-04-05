using System.ComponentModel;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.UserData.FileNavigation;

/// <summary>
/// A representation of a storage item on the file system that is read-only,
/// which is modified by the <see cref="ILaminarFileBrowser"/>
/// </summary>
public interface ILaminarStorageItem : INotifyPropertyChanged
{
    public FileSystemPath Path { get; }
    
    public bool IsEnabled { get; set; }
    
    public bool IsEffectivelyEnabled { get; }
    
    public bool NeedsName { get; set; }
    
    public ILaminarStorageFolder? ParentFolder { get; }
    
    public IObservableValue<long> SizeOnDisk { get; }
    
    public void Refresh();
}