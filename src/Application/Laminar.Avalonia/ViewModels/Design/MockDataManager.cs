using System;
using System.Collections.Generic;
using Laminar.Contracts.UserData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels.Design;

public class MockDataManager : IPersistentDataManager
{
    private static readonly FileSystemPath StaticPath = new(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Project Laminar", "Mock Data"));
    private readonly MockDataStore _dataStore = new(StaticPath);
    
    public IPersistentDataStore GetDataStore(DataStoreKey dataStoreKey)
    {
        return _dataStore;
    }

    public FileSystemPath Path => StaticPath;

    private class MockDataStore(FileSystemPath filePath) : IPersistentDataStore
    {
        private readonly Dictionary<string, object?> _dataStore = []; 
        
        public FileSystemPath FilePath { get; } = filePath;
        
        public DataReadResult<T> GetItem<T>(string key) where T : notnull
        {
            return _dataStore.TryGetValue(key, out var result) && result is T typedResult ? new DataReadResult<T>(typedResult) : default;
        }

        public IPersistentDataStore GetOrCreateChild(string childDataStoreName)
        {
            var retVal = new MockDataStore(FilePath.ChildPath(childDataStoreName));
            _dataStore.Add(childDataStoreName, retVal);
            return retVal;
        }

        public DataReadResult<object?> GetItem(string key, Type type)
        {
            return _dataStore.TryGetValue(key, out var result) && result?.GetType() == type ? new DataReadResult<object?>(result) : default;
        }

        public IObservableValue<object?> GetObservable(string key)
        {
            return new ObservableValue<object?>(_dataStore[key]);
        }

        public DataSaveResult SetItem<T>(string key, T value) where T : notnull
        {
            _dataStore[key] = value;
            return new DataSaveResult();
        }

        public DataSaveResult SetItem(string key, object? value)
        {
            _dataStore[key] = value;
            return new DataSaveResult();
        }

        public IPersistentDataStore InitializeDefaultValue<T>(string key, T value, object? deserializationContext = null) where T : notnull
        {
            _dataStore[key] = value;
            return this;
        }

        public IPersistentDataStore InitializeDefaultValue(string key, object? value, Type type, object? deserializationContext = null)
        {
            _dataStore[key] = value;
            return this;
        }

        public DataReadResult<object?> GetDefaultValue(string key)
        {
            throw new NotImplementedException();
        }
    }
}