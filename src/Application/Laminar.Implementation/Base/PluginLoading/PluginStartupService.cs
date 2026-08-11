using System.Runtime.Loader;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginStartupService(
    IFileSystem fileSystem,
    IPluginInstallContext pluginInstallContext,
    IPersistentDataManager dataManager,
    IPluginRepositoryStore pluginRepositoryStore) : IPluginStartupService
{
    private static readonly DataStoreKey InbuildRepositoriesDataStore
        = new("repositories", PersistentDataType.Json, AppContext.BaseDirectory);
    
    public async Task Initialize(FrontendDependency frontend, AssemblyLoadContext? defaultLoadContext)
    {
        pluginInstallContext.Configure(frontend, Platforms.All, defaultLoadContext);
        
        if (fileSystem.Exists(dataManager.GetDataStoreFilePath(InbuildRepositoriesDataStore)))
        {
            foreach (var dataPoint in dataManager.GetDataStore(InbuildRepositoriesDataStore)
                         ["repositories"].GetOrCreateCollection<IPersistentList>())
            {
                _ = pluginRepositoryStore.AddFromPersistentDictionary(dataPoint.GetOrCreateCollection<IPersistentDictionary>());
            }
        }

        foreach (var dataPoint in dataManager.GetDataStore(DataStoreKey.Settings)
                     ["plugin-repositories"].GetOrCreateCollection<IPersistentList>())
        {
            _ = pluginRepositoryStore.AddFromPersistentDictionary(dataPoint.GetOrCreateCollection<IPersistentDictionary>());
        }
        
        // foreach (var installedPlugin in settings["installed-plugins"].GetOrCreateCollection<IPersistentList>())
        // {
        //     var persistentDictionary = installedPlugin.GetOrCreateCollection<IPersistentDictionary>();
        //     var id = persistentDictionary["id"].GetValue<string>().Value;
        //     var version = persistentDictionary["version"].GetValue<SemanticVersion>().Value;
        //     if (!pluginRepositoryStore.TryGetPluginInfoFromId(id, out var pluginInfo) 
        //         || !pluginInfo.HasVersion(version))
        //     {
        //         await exceptionHandler.OnExceptionAsync(new CannotFindPluginException(id, version));
        //         continue;
        //     }
        //
        //     await pluginInstaller.TryGetPluginAssembly(pluginInfo, version);
        // }
        
        // foreach (var pluginDirectory in fileSystem.EnumerateChildren(PluginPath).Where(fileSystem.IsDirectory))
        // {
        //     foreach (var registeredPlugin in pluginLoader.LoadFrom(pluginDirectory, frontend, defaultLoadContext))
        //     {
        //         pluginRegistry.RegisterPlugin(registeredPlugin);
        //     }
        // }
    }
}