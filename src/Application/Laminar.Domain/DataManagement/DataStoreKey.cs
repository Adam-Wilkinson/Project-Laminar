using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.DataManagement;

public record DataStoreKey(string Name, PersistentDataType DataType, FileSystemPath Location)
{
    public static DataStoreKey PersistentData { get; } = new("PersistentData", PersistentDataType.Json, DataLocations.RoamingDataFolder);
    public static DataStoreKey Settings { get; } = new("Settings", PersistentDataType.Json, DataLocations.RoamingDataFolder);
    public static DataStoreKey ToolProperties { get; } = new("ToolProperties", PersistentDataType.Json, DataLocations.RoamingDataFolder);
}

public enum PersistentDataType
{
    Json = 1,
}