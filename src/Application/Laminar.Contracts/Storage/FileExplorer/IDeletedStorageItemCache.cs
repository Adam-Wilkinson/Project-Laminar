using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IDeletedStorageItemCache
{
    public void RegisterPotentialDeletion(ILaminarStorageItem potentialDeletion);

    public ILaminarStorageItem? TryFindAndRemove(FileSystemPath mightExist);
    
    public void CommitDeletions();
}