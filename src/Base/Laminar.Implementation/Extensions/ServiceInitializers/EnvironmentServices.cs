using Laminar.Contracts.Base;
using Laminar.Contracts.Base.Settings;
using Laminar.Domain.Notification;
using Laminar.Implementation.Base;
using Laminar.Implementation.Base.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

internal static class EnvironmentServices
{
    public static IServiceCollection AddEnvironmentServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ITypeInfoStore, TypeInfoStore>();

        serviceCollection.AddSingleton<IUserPreferenceManager, UserPreferenceManager>();

        serviceCollection.AddSingleton<IClassInstancer, ClassInstancer>();

        serviceCollection.AddSingleton<INotifyCollectionChangedHelper, NotifyCollectionChangedHelper>();

        return serviceCollection;
    }
}
