namespace Laminar.PluginFramework.Serialization;

public interface ISerializable<out TSelf, TSerialized>
{
    public TSerialized Serialize();
    
    public static abstract TSelf Deserialize(TSerialized serialized, object? deserializationContext = null);
}