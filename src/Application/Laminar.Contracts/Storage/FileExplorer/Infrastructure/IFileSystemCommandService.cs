namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

public interface IFileSystemCommandService
{
    public void Move(IFileSystemItem item, IFileSystemFolder newParent, int newIndex);

    public void Rename(IFileSystemItem item, string newNameAndExtension);

    public void Delete(IFileSystemItem item);

    public IFileSystemFile AddFile(IFileSystemFolder parent, string fileNameAndExtension, int indexInParent);

    public IFileSystemFolder AddFolder(IFileSystemFolder parent, string folderName, int indexInParent);
}