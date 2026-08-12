using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.PluginLoading.Repositories;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginSourceFactory(
    IFileSystem fileSystem,
    IExceptionHandler exceptionHandler,
    ISharedPluginContext sharedPluginContext,
    IPluginInstaller pluginInstaller) : IPluginSourceFactory
{
    private const string IdKey = "id";
    private const string PathKey = "path";
    private const string ProviderKey = "provider";

    public IPluginSource OfflineCache { get; } = new OfflineCacheSource(fileSystem, sharedPluginContext, pluginInstaller);

    public IPluginSource FromPersistentData(IPersistentDictionary persistentDictionary)
    {
        var id = persistentDictionary[IdKey].GetValue<string>().Value;
        var path = persistentDictionary[PathKey].GetValue<FileSystemPath>().Value;
        return persistentDictionary[ProviderKey].GetValue<string>().Value switch
        {
            "filesystem" or "local" => new LocalPluginSource(id, path, fileSystem, exceptionHandler, pluginInstaller),
            var unknown => throw new InvalidOperationException($"Cannot create plugin repository for type {unknown}")
        };
    }
}