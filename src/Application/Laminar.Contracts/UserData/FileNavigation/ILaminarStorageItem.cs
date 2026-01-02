using System.ComponentModel;

namespace Laminar.Contracts.UserData.FileNavigation;

public interface ILaminarStorageItem : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string Extension { get; }
    public string Path { get; }
    public bool IsEnabled { get; set; }
    public bool IsEffectivelyEnabled { get; }
    public bool NeedsName { get; set; }
    public void Delete();
    public void MoveTo(string newPath);
    public ILaminarStorageFolder ParentFolder { get; }
    public event EventHandler<IOException> ExceptionRaised;
}