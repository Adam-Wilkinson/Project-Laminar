using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.Implementation.Base.UserInterface;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

internal static class UserInterfaceServices
{
    public static IServiceCollection AddUserInterfaceServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUserInterfaceStore, UserInterfaceStore>();
        serviceCollection.AddSingleton<IReadOnlyUserInterfaceStore>(x => x.GetRequiredService<IUserInterfaceStore>());
        serviceCollection.AddSingleton<IUserInterfaceProvider, UserInterfaceProvider>();
        serviceCollection.AddSingleton<IDisplayFactory, DisplayFactory>();

        serviceCollection.AddSingleton<IUserActionManager, UserActionManager>();

        return serviceCollection;
    }
}
