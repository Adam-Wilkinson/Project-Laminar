using System;
using Laminar.PluginFramework.Exceptions;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar_PluginFramework.Primitives;

namespace Laminar_PluginFramework;

public static class Laminar
{
    private static bool initComplete = false;
    private static IObjectFactory? _serviceProvider;

    public static bool Init(IObjectFactory serviceProvider)
    {
        if (!initComplete)
        {
            _serviceProvider = serviceProvider;
            initComplete = true;
            return true;
        }

        return false;
    }

    public static T GetService<T>()
    {
        if (_serviceProvider is null)
        {
            throw new LaminarNotInitializedException();
        }

        return _serviceProvider.GetImplementation<T>();

        throw new CannotCreateServiceException<T>();
    }
}
