using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface ILaminarStorageItemFactory
{
    public ILaminarStorageItem FromPath(FileSystemPath path, ILaminarStorageFolder parent);

    public ILaminarStorageItem? TryGetExisting(FileSystemPath path);
    
    public ILaminarStorageRootFolder CreateRootFolder(FileSystemPath path);
    ILaminarStorageItem FromPersistentData(IPersistentDictionary persistentDictionary, ILaminarStorageFolder parent);
}