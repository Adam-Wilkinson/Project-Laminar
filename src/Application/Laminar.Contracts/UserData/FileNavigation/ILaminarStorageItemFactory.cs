namespace Laminar.Contracts.UserData.FileNavigation;

public interface ILaminarStorageItemFactory
{
    public ILaminarStorageItem FromPath(string path, ILaminarStorageFolder parent);
}