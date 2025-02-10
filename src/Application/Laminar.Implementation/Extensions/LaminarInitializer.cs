using System;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.PluginFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Extensions;

public static class LaminarInitializer
{
    public static IServiceProvider InitializeLaminar(this IServiceProvider serviceProvider, [CallerFilePath] string sourceFilePath = "")
    {
        LaminarFactory.ServiceProvider = serviceProvider;
        ((PluginLoader)serviceProvider.GetRequiredService<IPluginLoader>()).LoadInbuiltFromPath(sourceFilePath);
        serviceProvider.GetRequiredService<ILogger<None>>().LogTrace("Laminar Initialized");
        return serviceProvider;
    }
}