namespace Laminar.Contracts.Storage.PersistentData;

public interface IDecodingFactory<out TValue, in TData> where TData : IEncodableData where TValue : IEncodableDataOwner<TData>
{
    public TValue FromPersistentData(TData encodableData);
}