namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataTranscoder
{
    public string FileExtension { get; }
    
    public byte[] ElementToBytes(object value);
    
    object? BytesToElement(byte[] bytes);
    
    public object? EncodeElement(object value);
    
    public object? DecodeElement(object value, Type targetType);
    
}