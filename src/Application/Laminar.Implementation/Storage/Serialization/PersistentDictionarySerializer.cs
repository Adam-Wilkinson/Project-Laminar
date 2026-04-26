using System;
using System.Collections.Generic;
using System.Linq;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.Serialization;

public class PersistentDictionarySerializer(IServiceProvider serviceProvider) : TypeSerializer<IPersistentDictionary, Dictionary<string, object>>,
    INotifyingConditionalSerializer
{
    protected override Dictionary<string, object> SerializeTyped(IPersistentDictionary toSerialize)
        => ForceCast(toSerialize).InternalValues
            .ToDictionary(x => x.Key, x => x.Value.EncodedValue);

    protected override PersistentDictionary DeSerializeTyped(DeserializationRequest<IPersistentDictionary, Dictionary<string, object>> request)
    {
        PersistentDictionary returnValue = request.HasExistingValue
            ? request.ExistingValue as PersistentDictionary ?? throw new DeserializationError(
                new ArgumentException("Deserialization requires an existing PersistentDictionary", nameof(request)))
            : ActivatorUtilities.CreateInstance<PersistentDictionary>(serviceProvider); 
        
        foreach (var (key, value) in request.Serialized)
        {
            returnValue.GetPersistentData(key).EncodedValue = value;
        }
        
        return returnValue;
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