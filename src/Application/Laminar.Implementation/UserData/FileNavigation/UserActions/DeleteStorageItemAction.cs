using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Implementation.Base.ActionSystem;
using System;
using System.Threading.Tasks;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

internal class DeleteStorageItemAction(
    LaminarStorageItem item,
    ILaminarStorageRootFolder recyclingBin) : IUserAction
{
    private readonly CompoundAction _internalAction = new(
        new RenameStorageItemAction(GetDeletedName(item.Path.Name), item, recyclingBin), 
        new MoveStorageItemAction(item, recyclingBin, recyclingBin));

    public bool CanExecute => _internalAction.CanExecute;

    public event EventHandler? CanExecuteChanged
    {
        add => _internalAction.CanExecuteChanged += value;
        remove => _internalAction.CanExecuteChanged -= value;
    }

    public Task<IUserActionResult> Execute()
    {
        item.Refresh();
        return _internalAction.Execute();
    }

    private static string GetDeletedName(string name) => $"({DateTime.UtcNow.Ticks}) {name}";
}
