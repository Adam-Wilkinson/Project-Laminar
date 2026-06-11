namespace Laminar.Contracts.Storage.PersistentData;

public interface IEncodableDataFactory
{
    public IPersistentValue<T> GetValueWithDefault<T>(T defaultValue, Type? typeSerializationKeyOverride,
        object? deserializationContext) where T : notnull;

    public IPersistentValue<T> GetValueFromEncoded<T>(object encodedValue, IPersistentDataTranscoder transcoder,
        Type? typeSerializationKeyOverride, object? deserializationContext) where T : notnull;

    public IPersistentDataPoint GetDataPoint();
    
    public T GetEncodableData<T>() where T : class, IEncodablePersistentData;
}