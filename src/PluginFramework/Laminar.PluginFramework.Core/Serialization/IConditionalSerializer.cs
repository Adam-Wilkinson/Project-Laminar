namespace Laminar.PluginFramework.Serialization;
    
public interface IConditionalSerializer
{
    public Type? SerializedTypeOrNull(Type typeToSerialize);
    
    public object Serialize(object toSerialize);

    public object DeSerialize(DeserializationRequest request);
}