namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentList : IEncodablePersistentData, IReadOnlyList<IPersistentDataPoint> 
{
    public IPersistentDataPoint AddNext();

    public void Clear();
}