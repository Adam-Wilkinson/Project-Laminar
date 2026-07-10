using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Base;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.Implementation.Base.UserInterface;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.Implementation.Storage.Serialization;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

public static class LaminarServices
{
    public static IServiceCollection AddLaminarServices(
        this IServiceCollection services) => services
            .AddSingleton<IPersistentDataManager, PersistentDataManager>()
            .AddTransient<IPersistentDictionary, PersistentDictionary>()
            .AddTransient<IPersistentList, PersistentList>()
            .AddSingleton<IEncodableDataFactory, EncodableDataFactory>()
            .AddSingleton<ISerializer, Serializer>()
            
            .AddScoped<IUserActionManager, UserActionManager>()
            .AddSingleton<IUserActionChainSimplifier, UserActionChainSimplifier>()
            
            .AddSingleton<IDataInterfaceFactory, DataInterfaceFactory>()
            .AddSingleton<ITypeInfoStore, TypeInfoStore>()
            
            .AddSingleton<IPluginStartupService, PluginStartupService>()
            .AddSingleton<IPluginLoader, PluginLoader>()
            .AddSingleton<IPluginHostFactory, PluginHostFactory>()
            .AddSingleton<IWritablePluginRegistry, PluginRegistry>()
            .AddSingleton<IPluginRegistry>(provider => provider.GetRequiredService<IWritablePluginRegistry>())
        
            .AddSingleton<IExceptionHandler, ExceptionHandler>()
            
            .AddFileSystemServices()
            .AddScriptingServices();
}