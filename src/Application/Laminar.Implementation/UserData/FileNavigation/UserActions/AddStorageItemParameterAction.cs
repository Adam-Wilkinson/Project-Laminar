using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class AddStorageItemParameterAction<T>(IFileSystem fileSystem, ILaminarStorageItemFactory factory) : IParameterAction<ILaminarStorageFolder>
    where T : class, ILaminarStorageItem
{
    public IObservableValue<bool> CanExecute(ILaminarStorageFolder parameter) => new ObservableValue<bool>(true);
    
    public IUserAction Execute(ILaminarStorageFolder parameter) 
        => new AddDefaultStorageItemAction<T>(fileSystem, parameter, factory).Execute();
}