using System.Collections.Generic;
using System.Text.Json;
using Laminar.Contracts.UserData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData;

public class PersistentDataManager(ISerializer serializer, IFileSystem fileSystem, ILogger<IPersistentDataStore> dataStoreLogger, ILogger<JsonPersistentDataTranscoder> jsonTranscoderLogger) : IPersistentDataManager
{
    private readonly Dictionary<DataStoreKey, IPersistentDataStore> _dataStores = new();

    public FileSystemPath Path => DataLocations.RoamingDataFolder;

    public IPersistentDataStore GetDataStore(DataStoreKey dataStoreKey)
    {
        if (!fileSystem.Exists(Path))
        {
            fileSystem.CreateDirectory(Path);
        }
        
        if (_dataStores.TryGetValue(dataStoreKey, out var dataStore))
        {
            return dataStore;
        }

        var transcoder = dataStoreKey.DataType switch
        {
            PersistentDataType.Json => new JsonPersistentDataTranscoder(jsonTranscoderLogger),
            var unknown => throw new UnknownDataTypeException(unknown),
        };

        var file = fileSystem.GetFile(Path.ChildPath(dataStoreKey.Name + transcoder.FileExtension));

        var newDataStore = dataStoreKey.DataType switch
        {
            PersistentDataType.Json => new PersistentDataStore<JsonElement>(serializer, new JsonPersistentDataTranscoder(jsonTranscoderLogger), dataStoreLogger),
            var unknown => throw new UnknownDataTypeException(unknown),
        };

        newDataStore.RawData = file.Contents;
        newDataStore.DataChanged += (_, _) => file.Contents = newDataStore.RawData;
        // file.ContentsChanged += (_, _) => newDataStore.RawData = file.Contents;
        
        _dataStores[dataStoreKey] = newDataStore;
        return newDataStore;
    }
}