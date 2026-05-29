using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

public class FileExplorerActionDependencies
{
    public required IPersistentValue<List<FileSystemPath>> RootFolders { get; init; }
    
    public required IFileSystem FileSystem { get; init; } 

    public required ILaminarStorageItemFactory StorageItemFactory { get; init; }
    
    public required ILaminarStorageRootFolder RecyclingBin { get; init; }
}