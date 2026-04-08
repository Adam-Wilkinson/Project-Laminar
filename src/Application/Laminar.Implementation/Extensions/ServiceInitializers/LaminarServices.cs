using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Implementation.Base;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.Implementation.Base.UserInterface;
using Laminar.Implementation.UserData;
using Laminar.Implementation.UserData.FileNavigation;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

public static class LaminarServices
{
    public static IServiceCollection AddLaminarServices(this IServiceCollection services, FrontendDependency frontendDependency) => services
            .AddSingleton<IPersistentDataManager, PersistentDataManager>()
            .AddSingleton<ISerializer, Serializer>()
            .AddScoped<IUserActionManager, UserActionManager>()
            .AddSingleton<IDataInterfaceFactory, DataInterfaceFactory>()
            .AddSingleton<ITypeInfoStore, TypeInfoStore>()
            .AddSingleton<IPluginHostFactory, PluginHostFactory>()
            .AddSingleton<ILaminarStorageItemFactory, LaminarStorageItemFactory>()
            .AddSingleton<IDeletedStorageItemCache, DeletedStorageItemCache>()
            .AddSingleton<IFileSystem, FileSystem>()
            .AddScoped<ILaminarFileBrowser, LaminarFileBrowser>()
            .AddSingleton<IPluginLoader>(provider => new PluginLoader(frontendDependency, provider.GetRequiredService<IPluginHostFactory>(), provider.GetRequiredService<ILogger<IPluginHost>>()))
            .AddUserInterfaceServices()
            .AddScriptingServices();
}