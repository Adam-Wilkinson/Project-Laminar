using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Laminar.Contracts.UserData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData;

public class PersistentDataStore<TEncodedValue>(
    ISerializer serializer, 
    IPersistentDataTranscoder<TEncodedValue> persistentDataTranscoder, 
    ILogger<IPersistentDataStore> logger)
        : IPersistentDataStore, ISerializable<PersistentDataStore<TEncodedValue>, Dictionary<string, TEncodedValue>> 
    where TEncodedValue : notnull
{
    private readonly Dictionary<string, PersistentDataValue> _serializedDataCache = [];
    private byte[] _rawData = [];
    
    public byte[] RawData
    {
        get => _rawData;
        set
        {
            if (Equals(value, _rawData)) return;
            
            _rawData = value;
            persistentDataTranscoder.DecodeByteArray(_rawData, RegisterEncodedValue);
        }
    }

    public event EventHandler? DataChanged;

    public IPersistentDataStore CreateChild(string childDataStoreName)
    {
        var result = new PersistentDataStore<TEncodedValue>(serializer, persistentDataTranscoder, logger);
        InitializeDefaultValue(childDataStoreName, result, typeof(PersistentDataStore<TEncodedValue>), result);
        result.DataChanged += (_, _) =>
        {
            SetItem(childDataStoreName, result, typeof(PersistentDataStore<TEncodedValue>));
        };
        
        return result;
    }

    public DataReadResult<object?> GetItem(string key, Type type)
    {
        if (!_serializedDataCache.TryGetValue(key, out var persistentData))
        {
            return new DataReadResult<object?>(null, DataIoStatus.DataNotFound);
        }

        if (persistentData.ValueType != type)
        {
            return new DataReadResult<object?>(null, DataIoStatus.UnknownError);
        }
        
        return new DataReadResult<object?>(persistentData.Value);
    }

    public IObservableValue<object?> GetObservable(string key) => _serializedDataCache[key];

    public DataSaveResult SetItem(string key, object? value, Type type)
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

    public IPersistentDataStore InitializeDefaultValue(string key, object? value, Type type, object? deserializationContext = null)
    {
        if (!_serializedDataCache.TryGetValue(key, out var persistentValue))
        {
            persistentValue = new PersistentDataValue(serializer, persistentDataTranscoder, logger) { ValueName = key };
            _serializedDataCache[key] = persistentValue;
        }
        
        persistentValue.Initialize(value, type, deserializationContext);
        return this;
    }

    private void SyncToFile()
    {
        _rawData = persistentDataTranscoder.EncodeDictionary(_serializedDataCache, eachValue => eachValue.EncodedValue);
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
    
    
    public Dictionary<string, TEncodedValue> Serialize()
    {
        return new Dictionary<string, TEncodedValue>(_serializedDataCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.EncodedValue));
    }

    public static PersistentDataStore<TEncodedValue> Deserialize(Dictionary<string, TEncodedValue> serialized, object? deserializationContext = null)
    {
        if (deserializationContext is not PersistentDataStore<TEncodedValue> targetDataStore)
            throw new ArgumentException(nameof(deserializationContext));

        foreach (var (key, encodedValue) in serialized)
        {
            targetDataStore.RegisterEncodedValue(key, encodedValue);
        }

        return targetDataStore;
    }

    /// <summary>
    ///  Register a known encoded value. If the value is not yet initialized, the encoded value is stored,
    ///  and upon initialization is it decoded to give the correct value.
    /// </summary>
    private void RegisterEncodedValue(string name, TEncodedValue encodedValue)
    {
        if (!_serializedDataCache.TryGetValue(name, out var value))
        {
            value = new PersistentDataValue(serializer, persistentDataTranscoder, logger) { ValueName = name };
            _serializedDataCache[name] = value;
        }

        value.EncodedValue = encodedValue;
    }
    
    
    private class ValueNotInitializedException(string valueName) : Exception($"Value {valueName} has not been initialized");
    
    private class PersistentDataValue(ISerializer serializer, IPersistentDataTranscoder<TEncodedValue> transcoder, ILogger<IPersistentDataStore> logger) : IObservableValue<object?>
    {
        private TEncodedValue _encodedValue = default!;
        private bool _hasEncodedValue;
        private object? _defaultValue;
        private object? _value;
        private object? _deserializationContext;
        private Type? _valueType;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public event EventHandler<object?>? ValueChanged;

        public required string ValueName { get; init; }

        public Type ValueType => _valueType ?? throw new ValueNotInitializedException(ValueName);

        public TEncodedValue EncodedValue
        {
            get => _hasEncodedValue ? _encodedValue : throw new ValueNotInitializedException(ValueName);
            set
            {
                if (EqualityComparer<TEncodedValue>.Default.Equals(value, _encodedValue))
                {
                    return;
                }
                
                _encodedValue = value;
                _hasEncodedValue = true;
                
                if (_valueType is null)
                {
                    return;
                }

                if (!TrySetValueFromEncodedValue())
                {
                    SetEncodedValueFromValue();
                }
            }
        }

        public object? Value
        {
            get => _value ?? throw new ValueNotInitializedException(ValueName);
            set
            {
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

        public void Initialize(object? defaultValue, Type? valueType = null, object? deserializationContext = null)
        {
            _defaultValue = defaultValue;
            _valueType = valueType ?? _defaultValue?.GetType();
            _deserializationContext = deserializationContext;
            
            if (_hasEncodedValue && TrySetValueFromEncodedValue()) return;
            
            Value = _defaultValue;
            _hasEncodedValue = true;
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
        
        private bool TrySetValueFromEncodedValue()
        {
            if (transcoder.DecodeValue(EncodedValue, serializer.GetSerializedType(ValueType)) is not { } decodedValue)
            {
                logger.LogError("Error reading value {valueName}. Unable to decode value, the value will not be changed", ValueName);
                return false;
            }
            
            _value = serializer.DeserializeObject(decodedValue, ValueType, _deserializationContext);
            PropertyChanged?.Invoke(this, IObservableValueBase.ValueChangedEventArgs);
            ValueChanged?.Invoke(this, _value);
            return true;
        }
    }
}