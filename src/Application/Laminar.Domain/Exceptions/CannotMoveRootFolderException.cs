namespace Laminar.Domain.Exceptions;

public class CannotMoveRootFolderException(string folderName)
    : Exception($"Cannot move the folder '{folderName}' since it is a root folder")
{
    public string FolderName => folderName;
}