using Laminar.Domain.DataManagement;

namespace Laminar.Contracts.UserData;

public interface IPersistentDataManager
{
    public IPersistentDataStore GetDataStore(DataStoreKey dataStoreKey);
    
    public string Path { get; }
}