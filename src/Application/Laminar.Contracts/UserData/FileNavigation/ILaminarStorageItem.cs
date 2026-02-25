using System.ComponentModel;

namespace Laminar.Contracts.UserData.FileNavigation;

public interface ILaminarStorageItem : INotifyPropertyChanged
{
    public string Name { get; }
    public string Path { get; }
    public string Extension { get; }
    public bool IsEnabled { get; set; }
    public bool IsEffectivelyEnabled { get; }
    public bool NeedsName { get; set; }
    public ILaminarStorageFolder? ParentFolder { get; }
    // public void Delete();
    // public bool TryMoveTo(string newPath);
    public event EventHandler<IOException> ExceptionRaised;
    void Refresh();
}