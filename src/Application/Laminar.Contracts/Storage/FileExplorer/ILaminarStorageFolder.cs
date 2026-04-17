using Laminar.Domain.Notification;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface ILaminarStorageFolder : ILaminarStorageItem
{
    public IReadOnlyObservableCollection<ILaminarStorageItem> Contents { get; }

    public bool IsExpanded { get; set; }
}