namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileSystemCommandService
{
    public void Move(IFileSystemItem item, IFileSystemFolder newParent, int newIndex);

    public void Rename(IFileSystemItem item, string newNameAndExtension);

    public void Delete(IFileSystemItem item);

    public IFileSystemFile AddFile(IFileSystemFolder parent, int indexInParent, string fileNameAndExtension);

    public IFileSystemFolder AddFolder(IFileSystemFolder parent, int indexInParent, string folderName);
}