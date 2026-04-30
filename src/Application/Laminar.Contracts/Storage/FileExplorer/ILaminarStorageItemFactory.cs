using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface ILaminarStorageItemFactory
{
    public ILaminarStorageItem CreateChild(string itemNameAndExtension, ILaminarStorageFolder parent, bool isFolder);

    public ILaminarStorageItem? TryGetExisting(FileSystemPath path);
    
    public ILaminarStorageRootFolder CreateRootFolder(FileSystemPath path);
    
    ILaminarStorageItem FromPersistentData(IPersistentDictionary persistentDictionary, ILaminarStorageFolder parent);
}