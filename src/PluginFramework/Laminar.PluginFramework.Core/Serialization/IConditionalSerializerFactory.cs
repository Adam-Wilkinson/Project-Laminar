namespace Laminar.PluginFramework.Serialization;

public interface IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type type);
}