using Laminar.Contracts.Storage.FileExplorer;

namespace Laminar.Implementation.Storage.FileExplorer;

public static class FileSystemItemExtensions
{
    extension(IFileSystemItem item)
    {
        public IFileSystemRootFolder GetRootFolder()
        {
            IFileSystemItem? currentItem = item;
            while (currentItem is not null)
            {
                if (currentItem is IFileSystemRootFolder root)
                {
                    return root;
                }

                currentItem = currentItem.ParentFolder;
            }

            throw new InvalidOperationException($"Null parent: The file system item {item} does not have a root folder");
        }
    }
}