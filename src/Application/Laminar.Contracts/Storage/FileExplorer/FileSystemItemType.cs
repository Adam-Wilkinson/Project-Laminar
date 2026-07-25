using System.Reflection;
using Laminar.Contracts.Scripting;

namespace Laminar.Contracts.Storage.FileExplorer;

public class FileSystemItemType
{
    private static readonly Dictionary<string, FileSystemItemType> ItemsByExtension;
    
    public static FileSystemItemType FromExtension(string extension) => ItemsByExtension.GetValueOrDefault(extension, Unknown);
    
    public static FileSystemItemType Folder { get; } = new(true, "Folder", "Untitled Folder", string.Empty, null);

    public static FileSystemItemType Script { get; } = new(false, "Script", "Untitled Script", ".pls", typeof(IScript));

    public static FileSystemItemType Unknown { get; } = new(false, "Unknown", "Unknown File", string.Empty, null);

    static FileSystemItemType()
    {
        ItemsByExtension = typeof(FileSystemItemType)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Select(x => x.GetValue(null))
            .OfType<FileSystemItemType>()
            .Where(x => !string.IsNullOrWhiteSpace(x.Extension))
            .ToDictionary(x => x.Extension);
    }
    
    private FileSystemItemType(bool isFolder, string name, string defaultItemName, string extension, Type? contentsType)
    {
        IsFolder = isFolder;
        ItemTypeName = name;
        DefaultItemName = defaultItemName;
        Extension = extension;
        ContentsType = contentsType;
    }

    public string ItemTypeName { get; }
    
    public bool IsFolder { get; }
    
    public string DefaultItemName { get; }

    public string Extension { get; }

    public Type? ContentsType { get; }
}