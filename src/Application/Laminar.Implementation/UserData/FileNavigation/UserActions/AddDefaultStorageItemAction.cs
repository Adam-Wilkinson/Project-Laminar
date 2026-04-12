using System;
using System.IO;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class AddDefaultStorageItemAction<T>(
    IFileSystem fileSystem, 
    ILaminarStorageFolder parentFolder, 
    ILaminarStorageItemFactory factory,
    ILaminarStorageRootFolder recyclingBin) 
    : IUserAction where T : class, ILaminarStorageItem
{
    private static readonly (Type, string)[] DefaultItemNames =
    [
        (typeof(ILaminarStorageFolder), "Untitled Folder"),
        (typeof(ILaminarStorageItem), "Untitled Script.pls"),
    ];
    
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute => true;

    public Task<IUserActionResult> Execute()
    {
        FileSystemPath newItemPath = parentFolder.Path.ChildPath(GetDefaultItemName());
        ILaminarStorageItem newItem = factory.FromPath(newItemPath, parentFolder);
        newItem.NeedsName = true;
        return Task.FromResult(IUserActionResult.Success(new MoveStorageItemAction(newItem, recyclingBin, fileSystem)));
    }
    
    private static string GetDefaultItemName()
    {
        foreach (var (type, name) in DefaultItemNames)
        {
            if (type.IsAssignableFrom(typeof(T)))
            {
                return name;
            }
        }

        return "";
    }
}