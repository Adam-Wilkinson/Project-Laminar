using System;
using System.Collections.Generic;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.UserData.Serializers;

public class DataStoreSerializerFactory : IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PersistentDataStore<>))
        {
            return (IConditionalSerializer)Activator.CreateInstance(
                typeof(DataStoreSerializerFactory<>).MakeGenericType(type.GetGenericArguments()));
        }

        return null;
    }
}

public class DataStoreSerializerFactory<TEncodedValue> 
    : TypeSerializer<PersistentDataStore<TEncodedValue>, IEnumerable<(string, TEncodedValue)>> 
    where TEncodedValue : notnull
{
    protected override IEnumerable<(string, TEncodedValue)> SerializeTyped(PersistentDataStore<TEncodedValue> toSerialize)
    {
        return toSerialize.EncodedValues;
    }

    protected override PersistentDataStore<TEncodedValue> DeSerializeTyped(IEnumerable<(string, TEncodedValue)> serialized, object? deserializationContext = null)
    {
        if (deserializationContext is not PersistentDataStore<TEncodedValue> persistentStore)
            throw new ArgumentException(nameof(deserializationContext));

        persistentStore.EncodedValues = serialized;
        return persistentStore;
    }
}