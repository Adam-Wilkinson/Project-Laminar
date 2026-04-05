using System;
using System.Collections.Generic;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation;

public class DeletedStorageItemCache : IDeletedStorageItemCache
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

    private int HashDeletedItem(ILaminarStorageItem item) => item switch
    {
        ILaminarStorageFolder folder => HashCode.Combine(folder.Path.NameAndExtension, folder.SizeOnDisk.Value, folder.Contents.Count),
        _ => HashCode.Combine(item.Path.NameAndExtension, item.SizeOnDisk.Value)
    };
}