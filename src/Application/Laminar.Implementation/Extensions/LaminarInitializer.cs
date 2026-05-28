using Laminar.Contracts.Base.PluginLoading;
using Laminar.PluginFramework;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Extensions;

public static class LaminarInitializer
{
    public static IServiceProvider InitializeLaminar<T>(this IServiceProvider serviceProvider)
    {
        LaminarFactory.ServiceProvider = serviceProvider;
        serviceProvider.GetRequiredService<IPluginLoader>().EnsurePluginsLoaded();
        serviceProvider.GetRequiredService<ISerializer>().EnsureAssemblyInit(typeof(T).Assembly);
        serviceProvider.GetRequiredService<ILogger<None>>().LogTrace("Laminar Initialized with PluginFramework version {PluginFrameworkVersion}", PluginFrameworkInfo.Version);
        return serviceProvider;
    }
}