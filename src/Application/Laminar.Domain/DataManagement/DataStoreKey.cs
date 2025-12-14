namespace Laminar.Domain.DataManagement;

public record DataStoreKey(string Name, PersistentDataType DataType)
{
    public static DataStoreKey PersistentData { get; } = new("PersistentData", PersistentDataType.Json);
    public static DataStoreKey Settings { get; } = new("Settings", PersistentDataType.Json);
    public static DataStoreKey ToolProperties { get; } = new("ToolProperties", PersistentDataType.Json);
}

public enum PersistentDataType
{
    Json = 1,
}