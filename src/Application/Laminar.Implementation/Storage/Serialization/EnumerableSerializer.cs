using System;
using System.Collections.Generic;
using System.Linq;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class EnumerableSerializerFactory(ISerializer serializer) : IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var serializerType = typeof(EnumerableSerializer<,>).MakeGenericType(
                type.GenericTypeArguments[0], 
                serializer.GetSerializedType(type.GenericTypeArguments[0]));

            return Activator.CreateInstance(serializerType, serializer) as IConditionalSerializer;
        }

        return null;
    }
}

public class EnumerableSerializer<TElement, TSerializedElement>(ISerializer serializer)
    : TypeSerializer<IEnumerable<TElement>, IEnumerable<TSerializedElement>>
    where TElement : notnull
    where TSerializedElement : notnull
{
    protected override IEnumerable<TSerializedElement> SerializeTyped(IEnumerable<TElement> toSerialize) 
        => toSerialize.Select(x => (TSerializedElement)serializer.SerializeObject(x, typeof(TElement)));

    protected override IEnumerable<TElement> DeSerializeTyped(
        DeserializationRequest<IEnumerable<TElement>, IEnumerable<TSerializedElement>> request)
        => request.Serialized.Select(x => (TElement)serializer.DeserializeObject(new DeserializationRequest
        {
            Serialized = x,
            TargetType = typeof(TElement),
            Context = request.Context
        }));
}