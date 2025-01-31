using System;
using System.Collections.Generic;
using System.ComponentModel;
using Laminar.Contracts;
using Laminar.Contracts.UserData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData;

public class PersistentDataStore<TEncodedValue> : IPersistentDataStore where TEncodedValue : notnull
{
    private readonly ISerializer _serializer;
    private readonly IPersistentDataTranscoder<TEncodedValue> _persistentDataTranscoder;
    private readonly IFile _file;
    private readonly ILogger<IPersistentDataStore> _logger;
    
    private readonly Dictionary<string, PersistentDataValue> _serializedDataCache = [];
    
    public PersistentDataStore(
        IFile file, 
        ISerializer serializer, 
        IPersistentDataTranscoder<TEncodedValue> persistentDataTranscoder,
        ILogger<IPersistentDataStore> logger)
    {
        _logger = logger;
        _serializer = serializer;
        _persistentDataTranscoder = persistentDataTranscoder;
        _file = file;
        _file.ContentsChanged += FileChanged;
        FileChanged(null, EventArgs.Empty);
    }

    private void FileChanged(object? sender, EventArgs e)
    {
        _persistentDataTranscoder.DecodeByteArray(_file.Contents, (name, encodedValue) =>
        {
            if (!_serializedDataCache.TryGetValue(name, out var value))
            {
                value = new PersistentDataValue(_serializer, _persistentDataTranscoder, _logger) { ValueName = name };
                _serializedDataCache[name] = value;
            }

            value.EncodedValue = encodedValue;
        });
    }

    public string FilePath => _file.Path;

    public DataReadResult<T> GetItem<T>(string key)
        where T : notnull
    {
        var objectRead = GetItem(key, typeof(T));
        return new DataReadResult<T>(objectRead.Status == DataIoStatus.Success ? (T)objectRead.Result : default, objectRead.Status, objectRead.Exception);
    }

    public DataReadResult<object> GetItem(string key, Type type)
    {
        if (!_serializedDataCache.TryGetValue(key, out var persistentData))
        {
            return new DataReadResult<object>(null, DataIoStatus.DataNotFound);
        }

        if (persistentData.ValueType != type)
        {
            return new DataReadResult<object>(null, DataIoStatus.UnknownError);
        }
        
        return new DataReadResult<object>(persistentData.Value);
    }

    public IObservableValue<object> GetObservable(string key) => _serializedDataCache[key];

    public DataSaveResult SetItem<T>(string key, T value)
        where T : notnull
        => SetItem(key, value, typeof(T));

    public DataSaveResult SetItem(string key, object value, Type type)
    {
        if (!_serializedDataCache.TryGetValue(key, out var persistentValue))
        {
            return new DataSaveResult(DataIoStatus.DataNotFound);
        }
        
        persistentValue.Value = value;
        SyncToFile();
        return new DataSaveResult();
    }

    public DataSaveResult ResetToDefault(string key)
    {
        if (!_serializedDataCache.TryGetValue(key, out var persistentValue))
        {
            return new DataSaveResult(DataIoStatus.DataNotFound);
        }
        
        persistentValue.ResetToDefault();
        SyncToFile();
        return new DataSaveResult();
    }
    
    public IPersistentDataStore InitializeDefaultValue<T>(string key, T value, object? deserializationContext = null) where T : notnull 
        => InitializeDefaultValue(key, value, typeof(T), deserializationContext);

    public IPersistentDataStore InitializeDefaultValue(string key, object value, Type type, object? deserializationContext = null)
    {
        if (!_serializedDataCache.TryGetValue(key, out var persistentValue))
        {
            persistentValue = new PersistentDataValue(_serializer, _persistentDataTranscoder, _logger) { ValueName = key };
            _serializedDataCache[key] = persistentValue;
        }
        
        persistentValue.Initialize(value, type, deserializationContext);
        return this;
    }

    private void SyncToFile()
    {
        _file.Contents = _persistentDataTranscoder.EncodeDictionary(_serializedDataCache, eachValue => eachValue.EncodedValue);
    }
    
    private class ValueNotInitializedException(string valueName) : Exception($"Value {valueName} has not been initialized");
    
    private class PersistentDataValue(ISerializer serializer, IPersistentDataTranscoder<TEncodedValue> transcoder, ILogger<IPersistentDataStore> logger) : IObservableValue<object>
    {
        private TEncodedValue _encodedValue = default!;
        private bool _hasEncodedValue;
        private object? _defaultValue;
        private object? _value;
        private object? _deserializationContext;
        private Type? _valueType;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public event EventHandler<object>? ValueChanged;

        public required string ValueName { get; init; }

        public Type ValueType => _valueType ?? throw new ValueNotInitializedException(ValueName);

        public TEncodedValue EncodedValue
        {
            get => _hasEncodedValue ? _encodedValue : throw new ValueNotInitializedException(ValueName);
            set
            {
                if (value.Equals(_encodedValue))
                {
                    return;
                }
                
                _encodedValue = value;
                _hasEncodedValue = true;
                
                if (_valueType is null)
                {
                    return;
                }

                SetValueFromEncodedValue();
            }
        }

        public object Value
        {
            get => _value ?? throw new ValueNotInitializedException(ValueName);
            set
            {
                if (_value == value)
                {
                    return;
                }

                if (_valueType is null)
                {
                    throw new ValueNotInitializedException(ValueName);
                }

                if (!_valueType.IsInstanceOfType(value))
                {
                    throw new Exception($"The value {ValueName} is not of type {_valueType}");
                }
                
                _value = value;
                SetEncodedValueFromValue();
            }
        }

        public void Initialize(object defaultValue, Type? valueType = null, object? deserializationContext = null)
        {
            _defaultValue = defaultValue;
            _valueType = valueType ?? _defaultValue.GetType();
            _deserializationContext = deserializationContext;
            if (_hasEncodedValue)
            {
                SetValueFromEncodedValue();
            }
            else
            {
                Value = _defaultValue;
            }
        }

        public void ResetToDefault()
        {
            Value = _defaultValue ?? throw new ValueNotInitializedException(ValueName);
        }
        
        private void SetEncodedValueFromValue()
        {
            if (_value is null)
            {
                throw new ValueNotInitializedException(ValueName);
            }
            
            var serialized = serializer.SerializeObject(_value, ValueType);
            _encodedValue = transcoder.EncodeValue(serialized);
            PropertyChanged?.Invoke(this, IObservableValueBase.ValueChangedEventArgs);
            ValueChanged?.Invoke(this, _value);
        }
        
        private void SetValueFromEncodedValue()
        {
            if (transcoder.DecodeValue(EncodedValue, serializer.GetSerializedType(ValueType)) is not { } decodedValue)
            {
                logger.LogError("Error reading value {valueName}. Unable to decode value, the value will not be changed", ValueName);
                SetEncodedValueFromValue();
                return;
            }
            
            _value = serializer.DeserializeObject(decodedValue, ValueType, _deserializationContext);
            PropertyChanged?.Invoke(this, IObservableValueBase.ValueChangedEventArgs);
            ValueChanged?.Invoke(this, _value);
        }
    }
}