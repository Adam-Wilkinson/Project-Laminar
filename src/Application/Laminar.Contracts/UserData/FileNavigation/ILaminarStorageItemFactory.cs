namespace Laminar.Contracts.UserData.FileNavigation;

public interface ILaminarStorageItemFactory
{       
    public ILaminarStorageItem FromFileSystemInfo(FileSystemInfo fileSystemInfo, ILaminarStorageFolder parentFolder);
        
    public T FromPath<T>(string path, ILaminarStorageFolder? parentFolder = null) where T : class, ILaminarStorageItem;
}