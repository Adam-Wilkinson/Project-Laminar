namespace Laminar.Domain.Enums.ActionResolutions;

public enum DeleteRootFolderConfirmation
{
    /// <summary>
    /// The folder should be taken out of the list of root folders, but not deleted from disk
    /// </summary>
    RemoveRootFolder,
    
    /// <summary>
    /// The folder should be deleted from disk
    /// </summary>
    DeleteRootFolder,
}