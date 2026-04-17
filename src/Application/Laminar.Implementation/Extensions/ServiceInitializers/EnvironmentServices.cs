using Laminar.Contracts.Base;
using Laminar.Implementation.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

internal static class EnvironmentServices
{
    public static IServiceCollection AddEnvironmentServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ITypeInfoStore, TypeInfoStore>();
        
        return serviceCollection;
    }
}
