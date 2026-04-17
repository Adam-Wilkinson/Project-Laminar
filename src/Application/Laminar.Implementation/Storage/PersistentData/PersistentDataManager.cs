using System.Collections.Generic;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Exceptions;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataManager(
    ISerializer serializer, 
    IFileSystem fileSystem, 
    ILogger<PersistentDataValue> valueLogger,
    ILogger<JsonPersistentDataTranscoder> jsonTranscoderLogger) 
    : IPersistentDataManager
{
    private readonly Dictionary<DataStoreKey, IPersistentDataStore> _dataStores = new();

    public IPersistentDataNode GetDataStore(DataStoreKey dataStoreKey)
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
            PersistentDataType.Json => new PersistentDataStore(new JsonPersistentDataTranscoder(jsonTranscoderLogger), file, serializer, valueLogger),
            var unknown => throw new UnknownDataTypeException(unknown),
        };

        _dataStores[dataStoreKey] = newDataStore;
        return newDataStore.Root;
    }

    public IPersistentDataNode GetHeadlessNode() => new PersistentDataNode(serializer, valueLogger);
}