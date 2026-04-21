using System;
using System.Collections.Generic;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Exceptions;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataManager(
    IServiceProvider serviceProvider,
    ISerializer serializer, 
    IFileSystem fileSystem, 
    ILogger<JsonPersistentDataTranscoder> jsonTranscoderLogger) 
    : IPersistentDataManager
{
    private readonly Dictionary<DataStoreKey, IPersistentDataStore> _dataStores = new();

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
            PersistentDataType.Json => new PersistentDataStore(serviceProvider, new JsonPersistentDataTranscoder(jsonTranscoderLogger), file, serializer),
            var unknown => throw new UnknownDataTypeException(unknown),
        };

        _dataStores[dataStoreKey] = newDataStore;
        return newDataStore.Root;
    }

    public T GetHeadlessNode<T>() where T : IPersistentDataValueOwner => serviceProvider.GetRequiredService<T>();
}