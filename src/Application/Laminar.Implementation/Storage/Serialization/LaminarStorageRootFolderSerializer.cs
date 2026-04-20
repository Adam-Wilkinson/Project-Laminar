using System;
using System.Collections.Generic;
using System.Linq;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Implementation.Storage.FileExplorer;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

internal class LaminarStorageRootFolderSerializer : TypeSerializer<LaminarStorageRootFolder, Dictionary<string, object>>
{
    private const string Name = "Name";
    private const string IsEnabled = "IsEnabled";
    private const string IsExpanded = "IsExpanded";
    private const string Contents = "Contents";
    
    protected override Dictionary<string, object> SerializeTyped(LaminarStorageRootFolder toSerialize) 
        => SerializeLaminarStorageItem(toSerialize);

    protected override LaminarStorageRootFolder DeSerializeTyped(
        DeserializationRequest<LaminarStorageRootFolder, Dictionary<string, object>> request)
    {
        if (!request.HasExistingValue || request.ExistingValue is not { } folder || request.Context is not ILaminarStorageItemFactory factory)
        {
            throw new ArgumentException("Deserializing LaminarStorageItem requires a deserialization context of type ILaminarStorageItemFactory");
        }

        var serialized = request.Serialized;
        
        folder.IsEnabled = (bool)serialized[IsEnabled];
        folder.IsExpanded = (bool)serialized[IsExpanded];
        folder.ContentsInternal.Clear();
        foreach (var child in (IEnumerable<Dictionary<string, object>>)serialized[Contents])
        {
            folder.ContentsInternal.Add(DeserializeChild(child, folder, factory));
        }

        return folder;
    }

    private static Dictionary<string, object> SerializeLaminarStorageItem(ILaminarStorageItem toSerialize)
    {
        Dictionary<string, object> result = [];
        result[Name] = toSerialize.Path.NameAndExtension;
        result[IsEnabled] = toSerialize.IsEnabled;
        
        if (toSerialize is LaminarStorageFolder folder)
        {
            result[IsExpanded] = folder.IsExpanded;
            result[Contents] = folder.Contents.Select(SerializeLaminarStorageItem);
        }
        
        return result;
    }

    private static ILaminarStorageItem DeserializeChild(Dictionary<string, object> serialized, LaminarStorageFolder parent, ILaminarStorageItemFactory factory)
    {
        var childName = (string)serialized[Name];
        var returnValue = factory.FromPath(parent.Path.ChildPath(childName), parent);
        returnValue.IsEnabled = (bool)serialized[IsEnabled];
        
        if (returnValue is LaminarStorageFolder folder)
        {
            folder.IsExpanded = (bool)serialized[IsExpanded];
            folder.ContentsInternal.Clear();
            foreach (var child in (IEnumerable<Dictionary<string, object>>)serialized[Contents])
            {
                folder.ContentsInternal.Add(DeserializeChild(child, folder, factory));
            }
        }

        return returnValue;
    }
}