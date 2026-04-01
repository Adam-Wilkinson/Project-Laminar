using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.UserData;

public interface IPersistentDataManager
{
    public IPersistentDataStore GetDataStore(DataStoreKey dataStoreKey);
    
    public FileSystemPath Path { get; }
}