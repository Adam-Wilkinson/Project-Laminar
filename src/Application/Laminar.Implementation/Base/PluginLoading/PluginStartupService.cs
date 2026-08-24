using System.Runtime.Loader;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginStartupService(
    IFileSystem fileSystem,
    ISharedPluginContext sharedPluginContext,
    IPersistentDataManager dataManager,
    IPluginLibrary pluginLibrary,
    IPluginSourceFactory pluginSourceFactory) : IPluginStartupService
{
    private static readonly DataStoreKey InbuildRepositoriesDataStore
        = new("repositories", PersistentDataType.Json, Environment.CurrentDirectory);
    
    public Task Initialize(FrontendDependency frontend, AssemblyLoadContext? defaultLoadContext)
    {
        sharedPluginContext.Configure(frontend, Platforms.All, defaultLoadContext);

        _ = pluginLibrary.AddSource(pluginSourceFactory.OfflineCache);
        
        if (fileSystem.Exists(dataManager.GetDataStoreFilePath(InbuildRepositoriesDataStore)))
        {
            foreach (var dataPoint in dataManager.GetDataStore(InbuildRepositoriesDataStore)
                         ["repositories"].GetOrCreateCollection<IPersistentList>())
            {
                var newSource = pluginSourceFactory.FromPersistentData(dataPoint.GetOrCreateCollection<IPersistentDictionary>());
                _ = pluginLibrary.AddSource(newSource);
            }
        }

        foreach (var dataPoint in dataManager.GetDataStore(DataStoreKey.Settings)
                     ["plugin-repositories"].GetOrCreateCollection<IPersistentList>())
        {
            var newSource = pluginSourceFactory.FromPersistentData(dataPoint.GetOrCreateCollection<IPersistentDictionary>());
            _ = pluginLibrary.AddSource(newSource);
        }

        return Task.CompletedTask;
    }
}