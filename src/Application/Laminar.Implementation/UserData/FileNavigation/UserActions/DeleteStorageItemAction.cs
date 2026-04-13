using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Implementation.Base.ActionSystem;
using System;
using System.Threading.Tasks;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

internal class DeleteStorageItemAction(
    ILaminarStorageItem item,
    IFileSystem fileSystem,
    ILaminarStorageRootFolder recyclingBin) : IUserAction
{
    private readonly CompoundAction _internalAction = new(
        new RenameStorageItemAction(GetDeletedName(item.Path.Name), item, fileSystem, recyclingBin), 
        new MoveStorageItemAction(item, recyclingBin, fileSystem, recyclingBin));

    public bool CanExecute => _internalAction.CanExecute;

    public event EventHandler? CanExecuteChanged
    {
        add => _internalAction.CanExecuteChanged += value;
        remove => _internalAction.CanExecuteChanged -= value;
    }

    public Task<IUserActionResult> Execute()
    {
        return _internalAction.Execute();
    }

    private static string GetDeletedName(string name) => $"({DateTime.UtcNow.Ticks}) {name}".Replace(':', '-');
}
