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
    private readonly Dictionary<DataStoreKey, IResourceOnDisk<PersistentDictionaryOwner>> _dataStores = [];
    private readonly List<IResourceOnDisk<IEncodableDataOwner<IEncodableData>>> _allResources = [];
    private readonly PersistentDictionaryOwnerFactory _persistentDictionaryOwnerFactory = new();
    
    public IPersistentDictionary GetDataStore(DataStoreKey dataStoreKey)
    {
        if (_dataStores.TryGetValue(dataStoreKey, out var dataStore))
        {
            return dataStore.Resource.Data;
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
        var resource = GetResourceOnDisk(filePath, transcoder, _persistentDictionaryOwnerFactory);
        _dataStores[dataStoreKey] = resource;

        return resource.Resource.Data;
    }

    public IResourceOnDisk<TValue> GetResourceOnDisk<TValue, TData>(FileSystemPath filePath, IPersistentDataTranscoder transcoder, IDecodingFactory<TValue, TData> factory)
        where TValue : class, IEncodableDataOwner<TData>
        where TData : class, IEncodableData
    {
        var returnValue = new ResourceOnDisk<TValue, TData>(filePath, transcoder, fileSystem, dataFactory, factory);
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

    private class PersistentDictionaryOwnerFactory : IDecodingFactory<PersistentDictionaryOwner, IPersistentDictionary>
    {
        public PersistentDictionaryOwner FromPersistentData(IPersistentDictionary encodableData)
        {
            return new PersistentDictionaryOwner(encodableData);
        }
    }
    
    private class PersistentDictionaryOwner(IPersistentDictionary dictionary) 
        : IEncodableDataOwner<IPersistentDictionary>
    {
        public IPersistentDictionary Data => dictionary;
    }
}