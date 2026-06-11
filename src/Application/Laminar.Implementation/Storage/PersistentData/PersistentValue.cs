using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Laminar.Domain.Notification.Value;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.PersistentData;

internal class PersistentValue<T> : ObservableValueBase<T>, IPersistentValue<T> where T : notnull
{
    private readonly ISerializer _serializer;
    private readonly object? _deserializationContext;
    private readonly T? _defaultValue;
    private readonly Type _typeSerializationKey;
    
    private INotifySerializedValueChanged? _serializedValueChangedNotifier;
    private T _value;
    
    public static PersistentValue<T> FromEncodedValue(
        object encodedValue,
        Type? typeSerializationKeyOverride, 
        object? deserializationContext,
        ISerializer serializer,
        IPersistentDataTranscoder transcoder)
    {
        T initialValue = GetValueFromEncoded(encodedValue, serializer, transcoder,
            typeSerializationKeyOverride ?? typeof(T), deserializationContext, null);
        
        return new PersistentValue<T>(initialValue, typeSerializationKeyOverride, deserializationContext, serializer)
        {
            HasDefaultValue = false,
        };
    }
    
    public PersistentValue(T value, Type? typeSerializationKeyOverride, object? deserializationContext, ISerializer serializer)
    {
        _value = _defaultValue = value;
        _typeSerializationKey = typeSerializationKeyOverride ?? typeof(T);
        _deserializationContext = deserializationContext;
        _serializer = serializer;

        EstablishValue();
    }

    public override T Value
    {
        get => _value;
        set => SetAndRaise(ref _value, value);
    }

    public bool HasDefaultValue { get; private init; } = true;

    public T DefaultValue => HasDefaultValue && _defaultValue is not null
        ? _defaultValue
        : throw new Exception("This persistent value does not have a default");

    public void Reset()
    {
        if (HasDefaultValue && _defaultValue is not null) Value = _defaultValue;
    }
    
    protected override void BeforeValueChanged() => CleanupValue();
    
    protected override void AfterValueChanged() => EstablishValue();

    private void OnSerializedValueChanged(object? sender, EventArgs e) => OnInvalidated?.Invoke(sender, e);

    private void CleanupValue()
    {
        _serializedValueChangedNotifier?.SerializedValueChanged -= OnSerializedValueChanged;
        _serializedValueChangedNotifier?.Dispose();
        _serializedValueChangedNotifier = null;
    }

    private void EstablishValue()
    {
        if (_serializedValueChangedNotifier is not null)
            throw new InvalidOperationException("A previous value was not correctly cleaned");
        
        _serializedValueChangedNotifier = _serializer.GetSerializedValueChangedNotifier(Value, _typeSerializationKey);
        _serializedValueChangedNotifier.SerializedValueChanged += OnSerializedValueChanged;
        OnInvalidated?.Invoke(this, EventArgs.Empty);
    }
    
    public object Encode(IPersistentDataTranscoder transcoder)
    {
        var serialized = _serializer.SerializeObject(Value, _typeSerializationKey);
        return transcoder.EncodeElement(serialized) ?? throw new Exception();
    }

    public void Decode(IPersistentDataTranscoder transcoder, object encoded)
    {
        Value = GetValueFromEncoded(encoded, _serializer, transcoder, _typeSerializationKey, _deserializationContext,
            Value);
    }

    public event EventHandler? OnInvalidated;
    
    private static T GetValueFromEncoded(object encodedValue, ISerializer serializer, IPersistentDataTranscoder transcoder, Type typeSerializationKey,
        object? deserializationContext, object? existingValue)
    {
        var decoded = transcoder.DecodeElement(encodedValue, serializer.GetSerializedType(typeSerializationKey));

        if (decoded is null) throw new DeserializationError(new InvalidCastException());
        
        var newValue = serializer.DeserializeObject(new DeserializationRequest
        {
            Serialized = decoded,
            TargetType = typeSerializationKey,
            ExistingInstance = existingValue,
            Context = deserializationContext
        });

        if (newValue is not T typedValue)
        {
            throw new DeserializationError(new InvalidCastException());
        }

        return typedValue;
    }
}