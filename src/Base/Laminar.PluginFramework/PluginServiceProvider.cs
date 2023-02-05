using System;
using Laminar.PluginFramework.Exceptions;

namespace Laminar.PluginFramework;

internal static class PluginServiceProvider
{
#pragma warning disable CS8618 // Service Provider is always initialized by Laminar.Implementation
    internal static IServiceProvider ServiceProvider { get; set; }
#pragma warning restore CS8618 // Service Provider is always initialized by Laminar.Implementation

    public static T GetService<T>() 
    {
        if (ServiceProvider.GetService(typeof(T)) is T typedService)
        {
            return typedService;
        }

        throw new CannotCreateServiceException<T>();
    }
}
