using Laminar.Domain.DataManagement;

namespace Laminar.Contracts.UserData;

public interface IPersistentDataTranscoder<TEncodedValue>
{
    public string FileExtension { get; }

    public byte[] EncodeDictionary<T>(Dictionary<string, T> dict, Func<T, TEncodedValue> converter);

    public void DecodeByteArray(byte[] encoded, Action<string, TEncodedValue> decodeAction);

    public TEncodedValue EncodeValue(object value);

    public object? DecodeValue(TEncodedValue element, Type targetType);
}