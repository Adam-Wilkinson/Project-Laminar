using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;

namespace Laminar.Implementation.Storage.FileExplorer.Infrastructure;

internal sealed class FileSystemCommandService(
    IFileSystemGraph graph, 
    IFileSystem fileSystem) 
    : IFileSystemCommandService
{
    public void Move(IFileSystemItem item, IFileSystemFolder newParent, int newIndex)
    {
        var oldPath = item.Path;
        var newPath = newParent.Path.ChildPath(item.Path.NameAndExtension);
        
        graph.Move(item, newParent, newIndex);

        if (Equals(oldPath, newPath)) return;
        
        fileSystem.Move(oldPath, newPath);
    }

    public void Rename(IFileSystemItem item, string newNameAndExtension)
    { 
        if (item.Path.NameAndExtension == newNameAndExtension || item.Path.Parent is not { } parentPath) return;
        
        var oldPath = item.Path;
        var newPath = parentPath.ChildPath(newNameAndExtension);
        
        graph.Rename(item, newNameAndExtension);
        fileSystem.Move(oldPath, newPath);
    }

    public void Delete(IFileSystemItem item)
    {
        var oldPath = item.Path;
        graph.Delete(item);
        fileSystem.Delete(oldPath);
    }

    public IFileSystemFile AddFile(IFileSystemFolder parent, int indexInParent, string fileNameAndExtension)
    {
        var newItemPath = parent.Path.ChildPath(fileNameAndExtension);
        if (!fileSystem.Exists(newItemPath))
        {
            fileSystem.CreateFile(newItemPath).Close();
        }
        
        return graph.AddFile(parent, indexInParent, fileNameAndExtension);
    }

    public IFileSystemFolder AddFolder(IFileSystemFolder parent, int indexInParent, string folderName)
    {
        var newItemPath = parent.Path.ChildPath(folderName);
        if (!fileSystem.Exists(newItemPath))
        {
            fileSystem.CreateDirectory(newItemPath);   
        }
        
        return graph.AddFolder(parent, indexInParent, folderName);
    }
}