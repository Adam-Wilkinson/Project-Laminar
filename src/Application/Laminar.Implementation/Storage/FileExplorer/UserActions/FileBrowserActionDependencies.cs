using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

public class FileBrowserActionDependencies
{
    public required IPersistentValue<List<FileSystemPath>> RootFolders { get; init; }
    
    public required IFileSystem FileSystem { get; init; }
    
    public required IFileSystemRootFolder RecyclingBin { get; init; }

    public required IFileSystemGraph Graph { get; init; }
    
    public required IFileSystemItemRepository ItemRepository { get; init; }
}