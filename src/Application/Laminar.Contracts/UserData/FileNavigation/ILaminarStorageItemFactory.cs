using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.UserData.FileNavigation;

public interface ILaminarStorageItemFactory
{
    public ILaminarStorageItem FromPath(FileSystemPath path, ILaminarStorageFolder parent);

    public ILaminarStorageRootFolder CreateRootFolder(FileSystemPath path);
}