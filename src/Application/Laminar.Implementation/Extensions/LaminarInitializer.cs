using System.Runtime.Loader;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.PluginFramework;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Extensions;

public static class LaminarInitializer
{
    extension(IServiceProvider serviceProvider)
    {
        public IServiceProvider InitializeLaminar<T>(FrontendDependency frontendDependency,
            AssemblyLoadContext? defaultPluginLoadContext)
        {
            LaminarFactory.ServiceProvider = serviceProvider;
            serviceProvider.GetRequiredService<IPluginStartupService>().Initialize(frontendDependency, defaultPluginLoadContext);
            serviceProvider.GetRequiredService<ISerializer>().EnsureAssemblyInit(typeof(T).Assembly);
            serviceProvider.GetRequiredService<ILogger<None>>().LogTrace("Laminar initialized with PluginFramework version {PluginFrameworkVersion}", PluginFrameworkInfo.Version);
            return serviceProvider;
        }

        public async Task<IServiceProvider> InitializeLaminarAsync<T>(FrontendDependency frontendDependency,
            AssemblyLoadContext? defaultPluginLoadContext)
        {
            LaminarFactory.ServiceProvider = serviceProvider;
            await serviceProvider.GetRequiredService<IPluginStartupService>().Initialize(frontendDependency, defaultPluginLoadContext);
            serviceProvider.GetRequiredService<ISerializer>().EnsureAssemblyInit(typeof(T).Assembly);
            serviceProvider.GetRequiredService<ILogger<None>>().LogTrace("Laminar initialized with PluginFramework version {PluginFrameworkVersion}", PluginFrameworkInfo.Version);
            return serviceProvider;
        }
    }
}