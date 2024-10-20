using System;
using System.Diagnostics.CodeAnalysis;
using Laminar.PluginFramework.Exceptions;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.IO;

namespace Laminar.PluginFramework;

public static class LaminarFactory
{
#pragma warning disable CS8618 // Service Provider is always initialized by Laminar.Implementation
    internal static IServiceProvider ServiceProvider { get; set; }
#pragma warning restore CS8618 // Service Provider is always initialized by Laminar.Implementation

    public static INodeComponentFactory Component => GetService<INodeComponentFactory>();

    public static INodeIOFactory NodeIO => GetService<INodeIOFactory>();

    internal static T GetService<T>() 
    {
        if (ServiceProvider.GetService(typeof(T)) is T typedService)
        {
            return typedService;
        }

        throw new CannotCreateServiceException<T>();
    }
}
