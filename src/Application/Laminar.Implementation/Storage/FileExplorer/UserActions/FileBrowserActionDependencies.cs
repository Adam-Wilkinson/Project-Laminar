using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

public class FileBrowserActionDependencies
{
    public required IFileSystem FileSystem { get; init; }
    
    public required IFileSystemGraph Graph { get; init; }
    
    public required IFileSystemCommandService CommandService { get; init; }
}