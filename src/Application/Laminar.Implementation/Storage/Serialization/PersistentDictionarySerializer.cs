using System;
using System.Collections.Generic;
using System.Linq;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class PersistentDictionarySerializer : TypeSerializer<IPersistentDictionary, Dictionary<string, object>>,
    INotifyingConditionalSerializer
{
    protected override Dictionary<string, object> SerializeTyped(IPersistentDictionary toSerialize)
        => ForceCast(toSerialize).InternalValues
            .ToDictionary(x => x.Key, x => x.Value.EncodedValue);

    protected override PersistentDictionary DeSerializeTyped(DeserializationRequest<IPersistentDictionary, Dictionary<string, object>> request)
    {
        if (!request.HasExistingValue) throw new InvalidOperationException();
        
        PersistentDictionary persistentDictionary = ForceCast(request.ExistingValue);
        
        foreach (var (key, value) in request.Serialized)
        {
            persistentDictionary.GetPersistentData(key).EncodedValue = value;
        }
        
        return persistentDictionary;
    }

    public INotifySerializedValueChanged GetSerializedValueChangedNotifier(object target)
        => new DataNodeChangedNotifier((PersistentDictionary)target);

    private class DataNodeChangedNotifier : INotifySerializedValueChanged
    {
        public DataNodeChangedNotifier(PersistentDictionary node)
        {
            node.ChildValueChanged += (o, e) => SerializedValueChanged?.Invoke(o, e);
        }

        public event EventHandler? SerializedValueChanged;
    }
    
    private static PersistentDictionary ForceCast(IPersistentDictionary? persistentDictionary)
        => persistentDictionary as PersistentDictionary ?? throw new InvalidCastException();
}