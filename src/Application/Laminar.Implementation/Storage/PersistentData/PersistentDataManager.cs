using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

internal sealed class PersistentDataManager(
    IEncodableDataFactory dataFactory,
    IFileSystem fileSystem, 
    ILogger<JsonPersistentDataTranscoder> jsonTranscoderLogger) 
    : IPersistentDataManager
{
    private readonly Dictionary<DataStoreKey, IFileSyncedResource<PersistentDictionaryOwner>> _dataStores = [];
    private readonly List<IFileSyncedResource<IEncodableDataOwner>> _allResources = [];
    
    public IPersistentDictionary GetDataStore(DataStoreKey dataStoreKey)
    {
        if (_dataStores.TryGetValue(dataStoreKey, out var dataStore))
        {
            return dataStore.Resource.Dictionary;
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

        var filePath = dataStoreKey.Location.ChildPath(dataStoreKey.Name + transcoder.FileExtension);
        var resource = GetFileSyncedResource(new PersistentDictionaryOwner(dataFactory.GetEncodableData<IPersistentDictionary>()), transcoder, filePath);
        _dataStores[dataStoreKey] = resource;

        return resource.Resource.Dictionary;
    }

    public IFileSyncedResource<T> GetFileSyncedResource<T>(T value, IPersistentDataTranscoder transcoder,
        FileSystemPath filePath)
        where T : class, IEncodableDataOwner
    {
        var returnValue = new FileSyncedResource<T>(value, filePath, transcoder, fileSystem);
        _allResources.Add(returnValue);
        returnValue.OnDisposed += OnResourceDisposed;
        return returnValue;
        
        void OnResourceDisposed(object? sender, EventArgs e)
        {
            _allResources.Remove(returnValue);
            returnValue.OnDisposed -= OnResourceDisposed;
        }
    }

    public void ForgetDataStore(DataStoreKey dataStoreKey)
    {
        if (!_dataStores.TryGetValue(dataStoreKey, out var dataStore)) return;
        dataStore.Dispose();
        _dataStores.Remove(dataStoreKey);
    }

    public void Dispose()
    {
        while (_dataStores.Count > 0)
        {
            ForgetDataStore(_dataStores.Keys.First());
        }

        while (_allResources.Count > 0)
        {
            _allResources[0].Dispose();
        }
    }

    private class PersistentDictionaryOwner(IPersistentDictionary dictionary) : IEncodableDataOwner
    {
        public IPersistentDictionary Dictionary => dictionary;
        
        IEncodableData IEncodableDataOwner.Data => dictionary;
    }
}