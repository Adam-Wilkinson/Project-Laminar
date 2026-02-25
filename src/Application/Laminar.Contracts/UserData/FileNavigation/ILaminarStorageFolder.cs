using Laminar.Domain.Notification;

namespace Laminar.Contracts.UserData.FileNavigation;

public interface ILaminarStorageFolder : ILaminarStorageItem
{
    public IReadOnlyObservableCollection<ILaminarStorageItem> Contents { get; }
}