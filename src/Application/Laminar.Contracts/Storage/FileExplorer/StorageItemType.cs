using System.Reflection;
using Laminar.Contracts.Scripting;

namespace Laminar.Contracts.Storage.FileExplorer;

public class StorageItemType
{
    private static readonly Dictionary<string, StorageItemType> ItemsByExtension;
    
    public static StorageItemType FromExtension(string extension) => ItemsByExtension.GetValueOrDefault(extension, Unknown);
    
    public static StorageItemType Folder { get; } = new(true, "Folder", "Untitled Folder", string.Empty, null);

    public static StorageItemType Script { get; } = new(false, "Script", "Untitled Script", ".pls", typeof(IScript));

    public static StorageItemType Unknown { get; } = new(false, "Unknown", "Unknown File", string.Empty, null);

    static StorageItemType()
    {
        ItemsByExtension = typeof(StorageItemType)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Select(x => x.GetValue(null))
            .OfType<StorageItemType>()
            .Where(x => !string.IsNullOrWhiteSpace(x.Extension))
            .ToDictionary(x => x.Extension);
    }
    
    private StorageItemType(bool isFolder, string name, string defaultItemName, string extension, Type? contentsType)
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