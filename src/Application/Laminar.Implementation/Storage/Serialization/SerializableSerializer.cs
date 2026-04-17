using System;
using System.Linq;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.Serialization;

public partial class SerializableSerializer(ILogger<SerializableSerializer> logger) : IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISerializable<,>)) is not
            { } serializerInterface)
        {
            return null;
        }
        
        if (serializerInterface.GetGenericArguments()[0] != type)
        {
            LogFirstArgumentMismatch(logger, type, serializerInterface.GetGenericArguments()[0]);
            return null;
        }
            
        return (IConditionalSerializer)Activator.CreateInstance(
            typeof(SerializableSerializerInstance<,>).MakeGenericType(type, serializerInterface.GetGenericArguments()[1]));

    }

    [LoggerMessage(LogLevel.Warning, "The first argument of an implementation of ISerializable must be the type. The first argument of the implementation in {type} is {arg}.")]
    static partial void LogFirstArgumentMismatch(ILogger<SerializableSerializer> logger, Type type, Type arg);
}

public class SerializableSerializerInstance<TSerializable, TSerialized> : TypeSerializer<TSerializable, TSerialized>
    where TSerializable : ISerializable<TSerializable, TSerialized> 
    where TSerialized : notnull
{
    protected override TSerialized SerializeTyped(TSerializable toSerialize)
    {
        return toSerialize.Serialize();
    }

    protected override TSerializable DeSerializeTyped(TSerialized serialized, object? deserializationContext = null)
    {
        return TSerializable.Deserialize(serialized, deserializationContext);
    }
}