using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Laminar.Contracts.UserData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Helpers;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData;

/// <summary>
/// <para>
/// Handles serialization of a collection of string-keyed values. Converts all types via the serialization system into
/// savable types, before using the IPersistentDataTranscoder to save them to a byte array.
/// </para>
///
/// <para>
/// Makes a distinction between setting/getting values, and initializing them. Reads non-initialized
/// values as their raw encoded data, such that this value can be later initialized, at which point the stored data
/// is decoded and used. This means that persistent values read from files can be initialized and used at any time.
/// </para>
/// </summary>
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
            if (BytesHelper.Equals(value, _rawData)) return;
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
            SetItem(childDataStoreName, result);
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

    public DataSaveResult SetItem(string key, object? value)
    {
        if (!_serializedDataCache.TryGetValue(key, out var persistentValue))
        {
            return new DataSaveResult(DataIoStatus.DataNotFound);
        }
        
        persistentValue.Value = value;
        return new DataSaveResult();
    }

    public IPersistentDataStore InitializeDefaultValue(string key, object? value, Type type, object? deserializationContext = null)
    {
        GetPersistentData(key).Initialize(value, type, deserializationContext);
        return this;
    }

    public DataReadResult<object?> GetDefaultValue(string key)
    {
        if (!_serializedDataCache.TryGetValue(key, out PersistentDataValue? persistentValue))
        {
            return new DataReadResult<object?>(null, DataIoStatus.DataNotFound);
        }
        
        return new DataReadResult<object?>(persistentValue.DefaultValue);
    }
    
    private void SyncToFile()
    {
        _rawData = persistentDataTranscoder.EncodeDictionary(_serializedDataCache, eachValue => eachValue.EncodedValue);
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
    
    // TODO: This method of serialization seems vastly inefficient, but it works for now.
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
            targetDataStore.GetPersistentData(key).EncodedValue = encodedValue;
        }

        return targetDataStore;
    }
    
    private void RegisterEncodedValue(string name, TEncodedValue encodedValue)
    {
        GetPersistentData(name).EncodedValue = encodedValue;
    }

    private PersistentDataValue GetPersistentData(string key)
    {
        if (_serializedDataCache.TryGetValue(key, out var value))
        {
            return value;
        }

        value = new PersistentDataValue(serializer, persistentDataTranscoder, logger) { ValueName = key };
        value.ValueChanged += (_, _) => SyncToFile();
        _serializedDataCache[key] = value;
        return value;
    }
    
    
    private class ValueNotInitializedException(string valueName) : Exception($"Value {valueName} has not been initialized");
    
    private class PersistentDataValue(ISerializer serializer, IPersistentDataTranscoder<TEncodedValue> transcoder, ILogger<IPersistentDataStore> logger) : IObservableValue<object?>
    {
        private TEncodedValue _encodedValue = default!;
        private bool _hasEncodedValue;
        private object? _value;
        private object? _deserializationContext;
        private Type? _valueType;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public event EventHandler<ObservableValueChangedEventArgs<object?>>? ValueChanged;

        public required string ValueName { get; init; }

        public Type ValueType => _valueType ?? throw new ValueNotInitializedException(ValueName);

        public object? DefaultValue { get; private set; }

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
                
                var oldValue = _value;
                _value = value;
                SetEncodedValueFromValue();
                PropertyChanged?.Invoke(this, IObservableValueBase.ValueChangedEventArgs);
                ValueChanged?.Invoke(this, new ObservableValueChangedEventArgs<object?>(oldValue, _value));
            }
        }

        public void Initialize(object? defaultValue, Type? valueType = null, object? deserializationContext = null)
        {
            DefaultValue = defaultValue;
            _valueType = valueType ?? DefaultValue?.GetType();
            _deserializationContext = deserializationContext;
            
            if (_hasEncodedValue && TrySetValueFromEncodedValue()) return;
            
            Value = DefaultValue;
        }
        
        private void SetEncodedValueFromValue()
        {
            if (_value is null)
            {
                throw new ValueNotInitializedException(ValueName);
            }
            
            var serialized = serializer.SerializeObject(_value, ValueType);
            _encodedValue = transcoder.EncodeValue(serialized);
            _hasEncodedValue = true;
        }
        
        private bool TrySetValueFromEncodedValue()
        {
            if (transcoder.DecodeValue(EncodedValue, serializer.GetSerializedType(ValueType)) is not { } decodedValue)
            {
                logger.LogError("Error reading value {valueName}. Unable to decode value, the value will not be changed", ValueName);
                return false;
            }

            var oldValue = _value;
            _value = serializer.DeserializeObject(decodedValue, ValueType, _deserializationContext);
            PropertyChanged?.Invoke(this, IObservableValueBase.ValueChangedEventArgs);
            ValueChanged?.Invoke(this, new ObservableValueChangedEventArgs<object?>(oldValue, _value));
            return true;
        }
    }
}