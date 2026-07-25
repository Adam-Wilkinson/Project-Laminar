namespace Laminar.Contracts.Storage.PersistentData;

public interface IEncodableDataOwner<out T> where T : IEncodableData
{
    public T Data { get; }
}