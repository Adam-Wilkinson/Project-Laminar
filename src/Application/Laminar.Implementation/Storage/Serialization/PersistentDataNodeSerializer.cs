using System;
using System.Collections.Generic;
using System.Linq;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class PersistentDataNodeSerializer : TypeSerializer<PersistentDataNode, Dictionary<string, object>>
{
    protected override Dictionary<string, object> SerializeTyped(PersistentDataNode toSerialize)
        => toSerialize.InternalValues.ToDictionary(x => x.Key, x => x.Value.EncodedValue);

    protected override PersistentDataNode DeSerializeTyped(Dictionary<string, object> serialized, object? deserializationContext = null)
    {
        if (deserializationContext is not PersistentDataNode returnValue)
            throw new ArgumentException("Deserializing a persistent data node needs a persistent data node as context");

        foreach (var (key, value) in serialized)
        {
            returnValue.GetPersistentData(key).EncodedValue = value;
        }
        
        return returnValue;
    }
}