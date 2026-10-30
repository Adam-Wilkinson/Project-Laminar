using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Workspaces.PluginLoading.Repositories;

namespace Laminar.Implementation.Workspaces.PluginLoading;

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
        var id = persistentDictionary.GetValueOrThrow<string>(IdKey);
        var path = persistentDictionary.GetValueOrThrow<FileSystemPath>(PathKey);
        return persistentDictionary.GetValueOrThrow<string>(ProviderKey) switch
        {
            "filesystem" or "local" => new LocalPluginSource(id, path, fileSystem, exceptionHandler, pluginInstaller),
            var unknown => throw new InvalidOperationException($"Cannot create plugin repository for type {unknown}")
        };
    }
}