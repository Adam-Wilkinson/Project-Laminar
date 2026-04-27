using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Exceptions;

public class DeleteRootFolderException(FileSystemPath folderPath) : Exception("Attempt to delete root folder")
{
    public FileSystemPath FolderPath => folderPath;
}