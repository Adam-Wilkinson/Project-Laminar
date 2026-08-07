using System.Runtime.Loader;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginStartupService(
    IFileSystem fileSystem,
    // IPluginLoader pluginLoader, 
    // IWritablePluginRegistry pluginRegistry,
    IPersistentDataManager dataManager,
    IPluginRepositoryStore pluginRepositoryStore,
    IExceptionHandler exceptionHandler) : IPluginStartupService
{
    private static readonly DataStoreKey InbuildRepositoriesDataStore
        = new("repositories", PersistentDataType.Json, AppContext.BaseDirectory);
    
    public void Initialize(FrontendDependency frontend, AssemblyLoadContext? defaultLoadContext)
    {
        if (fileSystem.Exists(dataManager.GetDataStoreFilePath(InbuildRepositoriesDataStore)))
        {
            pluginRepositoryStore.AddFromPersistentList(dataManager.GetDataStore(InbuildRepositoriesDataStore)
                ["repositories"].GetOrCreateCollection<IPersistentList>());
        }

        var settings = dataManager.GetDataStore(DataStoreKey.Settings);
        
        pluginRepositoryStore.AddFromPersistentList(settings["plugin-repositories"]
                .GetOrCreateCollection<IPersistentList>());
        
        defaultLoadContext ??= AssemblyLoadContext.Default;

        foreach (var installedPlugin in settings["installed-plugins"].GetOrCreateCollection<IPersistentList>())
        {
            var persistentDictionary = installedPlugin.GetOrCreateCollection<IPersistentDictionary>();
            var id = persistentDictionary["id"].GetValue<string>().Value;
            var version = persistentDictionary["version"].GetValue<SemanticVersion>().Value;
            if (!pluginRepositoryStore.TryFindVersionedPlugin(id, version, out var pluginInfo))
            {
                exceptionHandler.OnException(new CannotFindPluginException(id, version));
                continue;
            }
        }
        
        // foreach (var pluginDirectory in fileSystem.EnumerateChildren(PluginPath).Where(fileSystem.IsDirectory))
        // {
        //     foreach (var registeredPlugin in pluginLoader.LoadFrom(pluginDirectory, frontend, defaultLoadContext))
        //     {
        //         pluginRegistry.RegisterPlugin(registeredPlugin);
        //     }
        // }
    }
}