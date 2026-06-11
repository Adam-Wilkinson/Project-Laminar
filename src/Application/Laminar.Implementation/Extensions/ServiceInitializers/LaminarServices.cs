using System.Runtime.Loader;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Base;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.Implementation.Base.UserInterface;
using Laminar.Implementation.Storage.FileExplorer;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.Implementation.Storage.Serialization;
using Laminar.Implementation.Storage.IO;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

public static class LaminarServices
{
    public static IServiceCollection AddLaminarServices(
        this IServiceCollection services, 
        FrontendDependency frontendDependency,
        AssemblyLoadContext? defaultLoadContext) => services
            .AddSingleton<IPersistentDataManager, PersistentDataManager>()
            .AddTransient<IPersistentDictionary, PersistentDictionary>()
            .AddTransient<IPersistentList, PersistentList>()
            .AddSingleton<IEncodableDataFactory, EncodableDataFactory>()
            .AddSingleton<ISerializer, Serializer>()
            
            .AddScoped<IUserActionManager, UserActionManager>()
            .AddSingleton<IUserActionChainSimplifier, UserActionChainSimplifier>()
            
            .AddSingleton<IDataInterfaceFactory, DataInterfaceFactory>()
            .AddSingleton<ITypeInfoStore, TypeInfoStore>()
            
            .AddSingleton<IPluginHostFactory, PluginHostFactory>()
            .AddSingleton<IPluginLoader>(provider => ActivatorUtilities.CreateInstance<PluginLoader>(provider, frontendDependency, defaultLoadContext ?? AssemblyLoadContext.Default))
            
            .AddSingleton<ILaminarStorageItemFactory, LaminarStorageItemFactory>()
            .AddSingleton<IDeletedStorageItemCache, DeletedStorageItemCache>()
            .AddSingleton<ILaminarFileSystemMonitor, LaminarFileSystemMonitor>()
            .AddSingleton<IFileSystem, FileSystem>()
            .AddScoped<ILaminarFileBrowser, LaminarFileBrowser>()
            
            .AddSingleton<IExceptionHandler, ExceptionHandler>()
            .AddScriptingServices();
}