using System;
using System.Collections.Generic;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation;

internal class DeletedStorageItemCache : IDeletedStorageItemCache
{
    private static readonly TimeSpan DeletedItemMoveDetectionCooldown = new(0, 0, 2); 
    
    private readonly Dictionary<int, (ILaminarStorageItem item, DateTime timestamp)> _recentlyDeletedItems = [];
    
    public void RegisterPotentialDeletion(ILaminarStorageItem potentialDeletion)
    {
        _recentlyDeletedItems[HashDeletedItem(potentialDeletion)] = (potentialDeletion, DateTime.Now);
    }

    public ILaminarStorageItem? TryFind(ILaminarStorageItem mightExist)
    {
        if (_recentlyDeletedItems.TryGetValue(HashDeletedItem(mightExist), out var existingItem) 
            && DateTime.Now - existingItem.timestamp < DeletedItemMoveDetectionCooldown)
        {
            return existingItem.item;
        }

        return null;
    }

    public void Clear() => _recentlyDeletedItems.Clear();

    private static int HashDeletedItem(ILaminarStorageItem item)
    {
        int currentHash = item.Path.NameAndExtension.GetHashCode();

        if (item is ILaminarStorageFolder folder)
        {
            int folderChildCount = 0;
            int fileChildCount = 0;

            foreach (var child in folder.Contents)
            {
                currentHash = HashCode.Combine(currentHash, child.Path.NameAndExtension);
                if (child is ILaminarStorageFolder)
                {
                    folderChildCount++;
                }
                else if (child is ILaminarStorageFile childFile)
                {
                    fileChildCount++;
                    currentHash = HashCode.Combine(currentHash, child.Path.NameAndExtension, childFile.SizeOnDisk);
                }
            }

            return HashCode.Combine(currentHash, folderChildCount, fileChildCount);
        }

        if (item is ILaminarStorageFile file)
        {
            return HashCode.Combine(currentHash, file.SizeOnDisk);
        }

        throw new InvalidOperationException();
    }
}