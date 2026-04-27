using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IDeletedStorageItemCache
{
    public void RegisterPotentialDeletion(ILaminarStorageItem potentialDeletion);

    public ILaminarStorageItem? TryFind(FileSystemPath mightExist);
    
    public void Clear();
}