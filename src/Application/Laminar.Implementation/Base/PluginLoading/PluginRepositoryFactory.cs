using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.PluginLoading.Repositories;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginRepositoryFactory(
    IFileSystem fileSystem,
    IExceptionHandler exceptionHandler) : IPluginRepositoryFactory
{
    private const string IdKey = "id";
    private const string PathKey = "path";
    private const string ProviderKey = "provider";
    
    public IPluginRepository FromPersistentData(IPersistentDictionary persistentDictionary)
    {
        var id = persistentDictionary[IdKey].GetValue<string>().Value;
        var path = persistentDictionary[PathKey].GetValue<FileSystemPath>().Value;
        return persistentDictionary[ProviderKey].GetValue<string>().Value switch
        {
            "filesystem" or "local" => new LocalPluginRepository(id, path, fileSystem, exceptionHandler),
            var unknown => throw new InvalidOperationException($"Cannot create plugin repository for type {unknown}")
        };
    }
}