using System;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.Implementation.UserData;
using Laminar.PluginFramework;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Extensions;

public static class LaminarInitializer
{
    public static IServiceProvider InitializeLaminar<T>(this IServiceProvider serviceProvider, [CallerFilePath] string sourceFilePath = "")
    {
        LaminarFactory.ServiceProvider = serviceProvider;
        ((PluginLoader)serviceProvider.GetRequiredService<IPluginLoader>()).LoadInbuiltFromPath(sourceFilePath);
        serviceProvider.GetRequiredService<ISerializer>().EnsureAssemblyInit(typeof(T).Assembly);
        serviceProvider.GetRequiredService<ILogger<None>>().LogTrace("Laminar Initialized");
        return serviceProvider;
    }
}