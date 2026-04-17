namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataTranscoder
{
    public string FileExtension { get; }
    
    public object? EncodeValue(object value);
    public object? DecodeValue(object value, Type targetType);
}

public interface IPersistentDataTranscoder<TEncodedValue>
{
    public string FileExtension { get; }

    public byte[] EncodeDictionary<T>(Dictionary<string, T> dict, Func<T, TEncodedValue> converter);

    public void DecodeByteArray(byte[] encoded, Action<string, TEncodedValue> decodeAction);

    public TEncodedValue EncodeValue(object value);

    public object? DecodeValue(TEncodedValue element, Type targetType);
}