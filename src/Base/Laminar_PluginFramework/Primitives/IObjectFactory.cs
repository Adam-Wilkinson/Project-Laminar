namespace Laminar_PluginFramework.Primitives
{
    public interface IObjectFactory
    {
        T GetImplementation<T>();

        // IObjectFactory RegisterImplementation<TInterface, TImplementation>() where TImplementation : class, TInterface;
    }
}