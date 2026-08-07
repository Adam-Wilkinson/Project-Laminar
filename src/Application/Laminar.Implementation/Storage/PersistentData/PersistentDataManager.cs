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
    private readonly Dictionary<DataStoreKey, IDataOnDisk<IPersistentDictionary>> _dataStores = [];
    private readonly List<IDataOnDisk<IEncodableData>> _allResources = [];
    private readonly JsonPersistentDataTranscoder _jsonTranscoder = new(jsonTranscoderLogger);
    
    public IPersistentDictionary GetDataStore(DataStoreKey dataStoreKey)
    {
        if (_dataStores.TryGetValue(dataStoreKey, out var dataStore))
        {
            return dataStore.Data;
        }
        
        if (!fileSystem.Exists(dataStoreKey.Folder))
        {
            fileSystem.CreateDirectory(dataStoreKey.Folder);
        }
        
        var transcoder = GetTranscoder(dataStoreKey.DataType);

        var filePath = dataStoreKey.Folder.ChildPath(dataStoreKey.Name + transcoder.FileExtension);
        var resource = GetDataOnDisk<IPersistentDictionary>(filePath, transcoder);
        _dataStores[dataStoreKey] = resource;

        return resource.Data;
    }

    public FileSystemPath GetDataStoreFilePath(DataStoreKey dataStoreKey) =>
        _dataStores.TryGetValue(dataStoreKey, out var dataOnDisk)
            ? dataOnDisk.Location
            : dataStoreKey.Folder.ChildPath(dataStoreKey.Name + GetTranscoder(dataStoreKey.DataType).FileExtension);

    public IDataOnDisk<TData> GetDataOnDisk<TData>(
        FileSystemPath filePath, 
        IPersistentDataTranscoder transcoder, 
        TData? initialValue = null)
        where TData : class, IEncodableData
    {
        initialValue ??= dataFactory.GetEncodableData<TData>();
        var returnValue = new DataOnDisk<TData>(filePath, transcoder, fileSystem, initialValue);
        _allResources.Add(returnValue);
        returnValue.OnDisposed += OnResourceDisposed;
        return returnValue;
        
        void OnResourceDisposed(object? sender, EventArgs e)
        {
            _allResources.Remove(returnValue);
            returnValue.OnDisposed -= OnResourceDisposed;
        }
    }

    public IPersistentDataTranscoder GetTranscoder(PersistentDataType dataType) => dataType switch
    {
        PersistentDataType.Json => _jsonTranscoder,
        _ => throw new UnknownDataTypeException(dataType),
    };

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
}