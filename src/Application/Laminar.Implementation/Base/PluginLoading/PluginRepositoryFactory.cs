using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.PluginLoading.Repositories;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginRepositoryFactory(IFileSystem fileSystem) : IPluginRepositoryFactory
{
    private const string IdKey = "id";
    private const string PathKey = "path";
    private const string TypeKey = "type";
    
    public IPluginRepository FromPersistentData(IPersistentDictionary persistentDictionary)
    {
        var id = persistentDictionary[IdKey].GetValue<string>().Value;
        var path = persistentDictionary[PathKey].GetValue<FileSystemPath>().Value;
        var newRepository = persistentDictionary[TypeKey].GetValue<string>().Value switch
        {
            "filesystem" => new LocalPluginRepository(id, path, fileSystem),
            var unknown => throw new InvalidOperationException($"Cannot create plugin repository for type {unknown}")
        };
        
        newRepository.Refresh();
        return newRepository;
    }
}