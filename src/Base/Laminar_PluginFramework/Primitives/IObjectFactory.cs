using System;

namespace Laminar_PluginFramework.Primitives
{
    public interface IObjectFactory
    {
        T GetImplementation<T>();

        object CreateInstance(Type type);

        T CreateInstance<T>();

        // IObjectFactory RegisterImplementation<TInterface, TImplementation>() where TImplementation : class, TInterface;
    }
}