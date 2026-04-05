namespace Laminar.Contracts.UserData.FileNavigation;

public interface IDeletedStorageItemCache
{
    public void RegisterPotentialDeletion(ILaminarStorageItem potentialDeletion);

    public void Clear();
    
    public ILaminarStorageItem? TryFind(ILaminarStorageItem mightExist);
}