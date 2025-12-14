using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Laminar.Contracts;
using Laminar.Contracts.UserData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Exceptions;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData;

public class PersistentDataManager(ISerializer serializer, IFileSystem fileSystem, ILogger<IPersistentDataStore> dataStoreLogger) : IPersistentDataManager
{
    private static readonly string StaticPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Project Laminar");
    private readonly Dictionary<DataStoreKey, IPersistentDataStore> _dataStores = new();
    
    static PersistentDataManager()
    {
        if (!Directory.Exists(StaticPath))
        {
            Directory.CreateDirectory(StaticPath);
        }
    }

    public string Path => StaticPath;

    public IPersistentDataStore GetDataStore(DataStoreKey dataStoreKey)
    {
        if (_dataStores.TryGetValue(dataStoreKey, out var dataStore))
        {
            return dataStore;
        }

        var transcoder = dataStoreKey.DataType switch
        {
            PersistentDataType.Json => new JsonPersistentDataTranscoder(),
            var unknown => throw new UnknownDataTypeException(unknown),
        };

        var file = fileSystem.GetFile(System.IO.Path.Combine(Path, dataStoreKey.Name + transcoder.FileExtension));

        var newDataStore = dataStoreKey.DataType switch
        {
            PersistentDataType.Json => new PersistentDataStore<JsonElement>(serializer, new JsonPersistentDataTranscoder(), dataStoreLogger),
            var unknown => throw new UnknownDataTypeException(unknown),
        };

        newDataStore.RawData = file.Contents;
        newDataStore.DataChanged += (_, _) => file.Contents = newDataStore.RawData;
        file.ContentsChanged += (_, _) => newDataStore.RawData = file.Contents;
        
        _dataStores[dataStoreKey] = newDataStore;
        return newDataStore;
    }
}