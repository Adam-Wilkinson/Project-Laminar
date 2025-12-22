using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.UserData.Serializers;

public class EnumerableSerializerFactory(ISerializer serializer) : IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type typeToSerialize)
    {
        if (typeToSerialize.GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)) is
                { } enumerableType
            && typeToSerialize.GetConstructor([]) is not null
            && typeToSerialize.GetMethod("Add") is { } addMethod
            && addMethod.GetParameters().Length == 1
            && addMethod.GetParameters()[0].ParameterType == enumerableType.GetGenericArguments()[0])
        {
            return Activator.CreateInstance(
                typeof(EnumerableSerializer<,,>).MakeGenericType(
                    enumerableType.GetGenericArguments()[0], 
                    serializer.GetSerializedType(enumerableType.GetGenericArguments()[0]), 
                    typeToSerialize), serializer) as IConditionalSerializer;
        }

        return null;
    }
}

public class EnumerableSerializer<TElement, TSerialized, TEnumerable>(ISerializer serializer) 
    : TypeSerializer<TEnumerable, SerializedEnumerable<TSerialized, TEnumerable>>
    where TEnumerable : IEnumerable<TElement> where TElement : notnull where TSerialized : notnull
{
    private readonly ISerializer _serializer = serializer;
    protected override SerializedEnumerable<TSerialized, TEnumerable> SerializeTyped(TEnumerable toSerialize)
    {
        return new(toSerialize.Select(x => (TSerialized)_serializer.SerializeObject(x, typeof(TElement))));
    }

    protected override TEnumerable DeSerializeTyped(SerializedEnumerable<TSerialized, TEnumerable> serialized, object? deserializationContext = null)
    {
        var enumerable = serialized.Select(x => _serializer.TryDeserialize<TElement>(x, deserializationContext));
        var result = Activator.CreateInstance<TEnumerable>();
        var addMethod = result!.GetType().GetMethod("Add");
        foreach (var element in enumerable)
        {
            addMethod!.Invoke(result, [element]);
        }

        return result;
    }
}

public class SerializedEnumerable<TSerializedElement, TEnumerable>(IEnumerable<TSerializedElement> serialized)
    : ICollection<TSerializedElement>
{
    private IEnumerable<TSerializedElement> _enumerable = serialized;

    public SerializedEnumerable() : this(new List<TSerializedElement>())
    {
    }
    
    public void Add(TSerializedElement toAdd) => ForceList().Add(toAdd);

    public void Clear() => ForceList().Clear();

    public bool Contains(TSerializedElement item) => ForceList().Contains(item);

    public void CopyTo(TSerializedElement[] array, int arrayIndex) => ForceList().CopyTo(array, arrayIndex);

    public bool Remove(TSerializedElement item) => ForceList().Remove(item);

    public int Count => ForceList().Count;
    public bool IsReadOnly => false;

    public IEnumerator<TSerializedElement> GetEnumerator() => _enumerable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private List<TSerializedElement> ForceList()
    {
        if (_enumerable is not List<TSerializedElement> enumerableList)
        {
            _enumerable = _enumerable.ToList();
        }
        
        return (List<TSerializedElement>)_enumerable;
    }
}
