using Laminar.Contracts.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.PersistentData;

public class EncodableDataFactory(ISerializer serializer, IServiceProvider serviceProvider) : IEncodableDataFactory
{
    public IPersistentValue<T> GetValueWithDefault<T>(T defaultValue, Type? typeSerializationKeyOverride,
        object? deserializationContext) where T : notnull 
        => new PersistentValue<T>(defaultValue, typeSerializationKeyOverride, deserializationContext, serializer);

    public IPersistentValue<T> GetValueFromEncoded<T>(object encodedValue, IPersistentDataTranscoder transcoder,
        Type? typeSerializationKeyOverride, object? deserializationContext) where T : notnull 
        => PersistentValue<T>.FromEncodedValue(encodedValue, typeSerializationKeyOverride, deserializationContext,
            serializer, transcoder);

    public IPersistentDataPoint GetDataPoint() => new PersistentDataPoint(this);
    
    public T GetEncodableData<T>() where T : class, IEncodablePersistentData => ActivatorUtilities.CreateInstance<T>(serviceProvider);
}