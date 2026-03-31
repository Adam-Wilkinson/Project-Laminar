namespace Laminar.Contracts.UserData.FileNavigation;

public interface ILaminarStorageItemFactory
{       
    public ILaminarStorageItem FromFileSystemInfo(FileSystemInfo fileSystemInfo, ILaminarStorageFolder parentFolder);

    public ILaminarStorageItem FromPath(string path, ILaminarStorageFolder parent);
    
    public T FromPath<T>(string path, ILaminarStorageFolder? parent = null) where T : class, ILaminarStorageItem;
}