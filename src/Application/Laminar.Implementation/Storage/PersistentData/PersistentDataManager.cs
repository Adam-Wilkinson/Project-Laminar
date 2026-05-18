using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

internal class PersistentDataManager(
    IServiceProvider serviceProvider,
    IFileSystem fileSystem, 
    ILogger<JsonPersistentDataTranscoder> jsonTranscoderLogger) 
    : IPersistentDataManager
{
    private readonly Dictionary<DataStoreKey, PersistentDataStore> _dataStores = new();

    public IPersistentDictionary GetDataStore(DataStoreKey dataStoreKey)
    {
        if (_dataStores.TryGetValue(dataStoreKey, out var dataStore))
        {
            return dataStore.Root;
        }
        
        if (!fileSystem.Exists(dataStoreKey.Location))
        {
            fileSystem.CreateDirectory(dataStoreKey.Location);
        }
        
        var transcoder = dataStoreKey.DataType switch
        {
            PersistentDataType.Json => new JsonPersistentDataTranscoder(jsonTranscoderLogger),
            var unknown => throw new UnknownDataTypeException(unknown),
        };

        var file = fileSystem.GetFile(dataStoreKey.Location.ChildPath(dataStoreKey.Name + transcoder.FileExtension));

        var newDataStore = dataStoreKey.DataType switch
        {
            PersistentDataType.Json => ActivatorUtilities.CreateInstance<PersistentDataStore>(serviceProvider, new JsonPersistentDataTranscoder(jsonTranscoderLogger), file),
            var unknown => throw new UnknownDataTypeException(unknown),
        };

        _dataStores[dataStoreKey] = newDataStore;
        return newDataStore.Root;
    }

    public void ForgetDataStore(DataStoreKey dataStoreKey)
    {
        if (_dataStores.TryGetValue(dataStoreKey, out var dataStore))
        {
            dataStore.Dispose();
            _dataStores.Remove(dataStoreKey);
        }
    }

    public T GetHeadlessNode<T>() where T : IPersistentDataValueOwner => serviceProvider.GetRequiredService<T>();

    public void Dispose()
    {
        foreach (var child in _dataStores.Values)
        {
            child.Dispose();
        }
        
        GC.SuppressFinalize(this);
    }
}