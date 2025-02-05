using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Laminar.Domain.DataManagement;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.UserData.Serializers;

public class DefaultSerializerFactory(ISerializer serializer) : IConditionalSerializerFactory
{
    private readonly ISerializer _serializer = serializer;
    
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (type.IsPrimitive || type == typeof(string))
        {
            return new PrimitiveSerializer();
        }

        return (IConditionalSerializer)Activator.CreateInstance(
            typeof(DefaultSerializer<>).MakeGenericType(type), 
            serializer);
    }
}

public class DefaultSerializer<T> : TypeSerializer<T, Dictionary<string, object>>
    where T : notnull
{
    private delegate void SetterDelegate(T sourceObject, object value);
    private delegate object GetterDelegate(T sourceObject);
    
    private readonly SerializablePropertyInfo[] _properties;
    private readonly int _propertyCount;
    private readonly ISerializer _serializer;

    public DefaultSerializer(ISerializer serializer)
    {
        _properties = typeof(T).GetProperties(BindingFlags.Public)
            .Where(propertyInfo => propertyInfo is { SetMethod: { } setter, GetMethod: { } getter, PropertyType: not T })
            .Select(propertyInfo => new SerializablePropertyInfo(
                propertyInfo.Name, 
                serializer.GetSerializedType(typeof(T)),
                propertyInfo.SetMethod!.CreateDelegate<SetterDelegate>(),
                propertyInfo.GetMethod!.CreateDelegate<GetterDelegate>()))
            .ToArray();
        
        _propertyCount = _properties.Length;
        _serializer = serializer;
    }
    
    protected override Dictionary<string, object> SerializeTyped(T toSerialize)
    {
        var retVal = new Dictionary<string, object>(_propertyCount);

        foreach (var property in _properties)
        {
            retVal.Add(property.PropertyName, _serializer.SerializeObject(property.Getter(toSerialize)));
        }
        
        return retVal;
    }

    protected override T DeSerializeTyped(Dictionary<string, object> serialized, object? deserializationContext = null)
    {
        var retVal = Activator.CreateInstance<T>();

        foreach (var property in _properties)
        {
            property.Setter(retVal, _serializer.DeserializeObject(serialized[property.PropertyName], property.PropertyType));
        }

        return retVal;
    }

    private record struct SerializablePropertyInfo(string PropertyName, Type PropertyType, SetterDelegate Setter, GetterDelegate Getter);
}

public class PrimitiveSerializer : IConditionalSerializer
{
    public Type? SerializedTypeOrNull(Type typeToSerialize) => typeToSerialize;

    public object Serialize(object toSerialize) => toSerialize;

    public object DeSerialize(object serialized, object? deserializationContext = null) => serialized;
}