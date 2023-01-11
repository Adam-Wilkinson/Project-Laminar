using Laminar.Contracts.Base.PluginLoading;
using Laminar.Implementation.Base.PluginLoading;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

internal static class PluginServices
{
    public static IServiceCollection AddPluginServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IPluginHostFactory, PluginHostFactory>();

        return serviceCollection;
    }
}
