using System;

namespace Laminar.PluginFramework;

internal static class PluginServiceProvider
{
#pragma warning disable CS8618 // Service Provider is always initialized by Laminar.Implementation
    internal static IServiceProvider ServiceProvider { get; set; }
#pragma warning restore CS8618 // Service Provider is always initialized by Laminar.Implementation
}
