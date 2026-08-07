using System.Runtime.Loader;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.PluginFramework;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Extensions;

public static class LaminarInitializer
{
    public static IServiceProvider InitializeLaminar<T>(
        this IServiceProvider serviceProvider,
        FrontendDependency frontendDependency,
        AssemblyLoadContext? defaultPluginLoadContext)
    {
        LaminarFactory.ServiceProvider = serviceProvider;
        serviceProvider.GetRequiredService<IPluginStartupService>().Initialize(frontendDependency, defaultPluginLoadContext);
        serviceProvider.GetRequiredService<ISerializer>().EnsureAssemblyInit(typeof(T).Assembly);
        serviceProvider.GetRequiredService<ILogger<None>>().LogTrace("Laminar initialized with PluginFramework version {PluginFrameworkVersion}", PluginFrameworkInfo.Version);
        return serviceProvider;
    }
    
    public static async Task<IServiceProvider> InitializeLaminarAsync<T>(
        this IServiceProvider serviceProvider,
        FrontendDependency frontendDependency,
        AssemblyLoadContext? defaultPluginLoadContext)
    {
        LaminarFactory.ServiceProvider = serviceProvider;
        await serviceProvider.GetRequiredService<IPluginStartupService>().Initialize(frontendDependency, defaultPluginLoadContext);
        serviceProvider.GetRequiredService<ISerializer>().EnsureAssemblyInit(typeof(T).Assembly);
        serviceProvider.GetRequiredService<ILogger<None>>().LogTrace("Laminar initialized with PluginFramework version {PluginFrameworkVersion}", PluginFrameworkInfo.Version);
        return serviceProvider;
    }
}