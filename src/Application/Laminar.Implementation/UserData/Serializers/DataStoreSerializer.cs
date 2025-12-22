using System;
using System.Collections.Generic;
using System.Text;
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
    : TypeSerializer<PersistentDataStore<TEncodedValue>, string> 
    where TEncodedValue : notnull
{
    protected override string SerializeTyped(PersistentDataStore<TEncodedValue> toSerialize)
    {
        return Encoding.UTF8.GetString(toSerialize.RawData);
    }

    protected override PersistentDataStore<TEncodedValue> DeSerializeTyped(string serialized, object? deserializationContext = null)
    {
        if (deserializationContext is not PersistentDataStore<TEncodedValue> persistentStore)
            throw new ArgumentException(nameof(deserializationContext));

        persistentStore.RawData = Encoding.UTF8.GetBytes(serialized);
        return persistentStore;
    }
}