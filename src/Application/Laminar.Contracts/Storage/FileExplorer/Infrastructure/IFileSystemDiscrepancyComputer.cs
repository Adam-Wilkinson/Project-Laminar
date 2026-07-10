namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

/// <summary>
/// Ensures that the given storage graphs and root folders are synchronized with the 
/// </summary>
public interface IFileSystemDiscrepancyComputer
{
    /// <summary>
    /// Computes the file system events that describe the difference between a folder and that folder on disk
    /// </summary>
    /// <param name="folder">The folder to be examined</param>
    /// <returns>A list of file system evens that represents the difference between the folder and its representation on disk</returns>
    public IEnumerable<FileSystemEvent> ComputeFolderDiscrepancies(IFileSystemFolder folder);
}