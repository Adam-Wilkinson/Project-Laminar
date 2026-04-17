namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataTranscoder
{
    public string FileExtension { get; }
    
    public byte[] ToBytes(object value);
    public object? EncodeElement(object value);
    public object? DecodeElement(object value, Type targetType);
    T? FromBytes<T>(byte[] bytes);
}

public static class IPersistentDataTranscoderExtensions
{
    extension(IPersistentDataTranscoder transcoder)
    {
        public T? DecodeValue<T>(object value) => transcoder.DecodeElement(value, typeof(T)) is T typedValue ? typedValue : default;
    }
}

public interface IPersistentDataTranscoder<TEncodedValue>
{
    public string FileExtension { get; }

    public byte[] EncodeDictionary<T>(Dictionary<string, T> dict, Func<T, TEncodedValue> converter);

    public void DecodeByteArray(byte[] encoded, Action<string, TEncodedValue> decodeAction);

    public TEncodedValue EncodeValue(object value);

    public object? DecodeValue(TEncodedValue element, Type targetType);
}