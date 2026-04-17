namespace Laminar.Contracts.Storage.FileExplorer;

public interface IDeletedStorageItemCache
{
    public void RegisterPotentialDeletion(ILaminarStorageItem potentialDeletion);

    public void Clear();
    
    public ILaminarStorageItem? TryFind(ILaminarStorageItem mightExist);
}