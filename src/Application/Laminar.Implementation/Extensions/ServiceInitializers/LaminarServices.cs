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
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using FileSystem = Laminar.Implementation.Storage.IO.FileSystem;
using LaminarFileSystemMonitor = Laminar.Implementation.Storage.FileExplorer.LaminarFileSystemMonitor;
using LaminarStorageItemFactory = Laminar.Implementation.Storage.FileExplorer.LaminarStorageItemFactory;

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
            .AddSingleton<ILaminarFileSystemMonitor, LaminarFileSystemMonitor>()
            .AddSingleton<IFileSystem, FileSystem>()
            .AddScoped<ILaminarFileBrowser, LaminarFileBrowser>()
            .AddSingleton<IPluginLoader>(provider => ActivatorUtilities.CreateInstance<PluginLoader>(provider, frontendDependency))
            .AddUserInterfaceServices()
            .AddScriptingServices();
}