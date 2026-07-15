using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

/// <summary>
/// Manages the in-memory representation of the file system graph, which may consist of several trees. This class does NOT touch the file system.
/// File system graph mutations can only happen via this class, but it is for internal use only.
/// <see cref="IFileSystemCommandService"/> is the correct entrypoint for modifying the file system.
/// </summary>
public interface IFileSystemGraph
{
    /// <summary>
    /// The root folders of the graph, each of which represents a distinct file tree
    /// </summary>
    public IFileSystemRoots Roots { get; }

    /// <summary>
    /// <para>A recycling bin exposed separately from the <see cref="Roots"/> collection</para>
    /// <para>NOTE: The graph operations don't move items here themselves; delete is a destructive operation. To use the recycling bin, use move operations. </para>
    /// </summary>
    public IFileSystemRootFolder RecyclingBin { get; }

    /// <summary>
    /// Moves an item from one place within the tree to another
    /// </summary>
    /// <param name="item">The item to move</param>
    /// <param name="newParent">The new parent of the item</param>
    /// <param name="newIndex">The target index of the item in the new parent's children</param>
    public void Move(IFileSystemItem item, IFileSystemFolder newParent, int newIndex);

    /// <summary>
    /// Renames a file
    /// </summary>
    /// <param name="item">The file to rename</param>
    /// <param name="newNameAndExtension">The new name of the file, including any file extensions</param>
    public void Rename(IFileSystemItem item, string newNameAndExtension);

    /// <summary>
    /// Deletes a file from the file tree
    /// </summary>
    /// <param name="item">The item to delete</param>
    public void Remove(IFileSystemItem item);

    public IFileSystemFolder AddFolder(IFileSystemFolder parent, int indexInParent, string name);
    
    public IFileSystemFile AddFile(IFileSystemFolder parent, int indexInParent, string nameAndExtension);

    public IFileSystemItem AddFromPersistentData(IFileSystemFolder parent, IPersistentDictionary persistentData);
}