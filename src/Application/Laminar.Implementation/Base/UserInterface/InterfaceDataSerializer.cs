using System.ComponentModel;
using Laminar.PluginFramework.Serialization;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Base.UserInterface;

public class SourcedInterfaceDataSerializerFactory(ISerializer serializer) : IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (GetSourcedInterfaceDataType(type) is { } sourcedInterfaceDataType)
        {
            return Activator.CreateInstance(
                typeof(InterfaceDataSerializer<>).MakeGenericType(sourcedInterfaceDataType.GetGenericArguments()[0]),
                serializer) as IConditionalSerializer;
        }

        return null;
    }

    private static Type? GetSourcedInterfaceDataType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IInterfaceData<>))
        {
            return type;
        }

        if (type.IsAssignableTo(typeof(IInterfaceData))
            && type.GetInterfaces().FirstOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IInterfaceData<>))
                is { } sourcedInterfaceDataType)
        {
            return sourcedInterfaceDataType;
        }

        return null;
    }
}

public class InterfaceDataSerializer<T>(ISerializer serializer) : TypeSerializer<IInterfaceData<T>> where T : notnull
{
    public override Type SerializedType => serializer.GetSerializedType(typeof(T?));
    
    protected override object? SerializeOverride(IInterfaceData<T> toSerialize)
    {
        if (toSerialize is not IPersistenceOverrideInterfaceData<T> persistenceOverride)
        {
            return toSerialize.IsUserEditable ? serializer.SerializeObject(toSerialize.Value) : null;
        }

        if (persistenceOverride.PersistenceBehaviour == PersistenceBehaviour.Never ||
            persistenceOverride is { PersistenceBehaviour: PersistenceBehaviour.WhenUserEditable, IsUserEditable: false })
        {
            return null;
        }
        
        return serializer.SerializeObject(persistenceOverride.PersistentValue);
    }

    protected override IInterfaceData<T> DeSerializeOverride(DeserializationRequest request)
    {
        if (request.ExistingInstance is not IInterfaceData<T> existingInstance)
            throw new InvalidOperationException("Deserializing interface data requires existing value");

        var existingValue = existingInstance is IPersistenceOverrideInterfaceData<T> persistenceOverride
            ? persistenceOverride.PersistentValue
            : existingInstance.Value;

        if (request.Serialized is null)
        {
            return existingInstance;
        }
        
        T newValue = serializer.DeserializeObject(request with
        {
            TargetType = typeof(T),
            ExistingInstance = existingValue,
        }) is T deserialized ? deserialized : throw new InvalidCastException();
        
        if (existingInstance.IsUserEditable)
        {
            existingInstance.Value = newValue;
        }
        else
        {
            existingInstance.SetValue(newValue);
        }
        
        return existingInstance;
    }

    protected override INotifySerializedValueChanged GetSerializedValueChangedNotifier(IInterfaceData<T> target) 
        => new SerializedValueChangedListener(target);

    private class SerializedValueChangedListener : INotifySerializedValueChanged
    {
        private readonly IInterfaceData<T> _target;
        
        public SerializedValueChangedListener(IInterfaceData<T> target)
        {
            _target = target;
            _target.PropertyChanged += TargetPropertyChanged;
        }

        private void TargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_target is not IPersistenceOverrideInterfaceData<T> persistenceOverride)
            {
                if (e.PropertyName == nameof(IInterfaceData<>.Value))
                {
                    SerializedValueChanged?.Invoke(this, EventArgs.Empty);
                }
                
                return;
            }

            if (e.PropertyName == nameof(IPersistenceOverrideInterfaceData<>.PersistenceBehaviour))
            {
                SerializedValueChanged?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            if (persistenceOverride is { PersistenceBehaviour: PersistenceBehaviour.Always } 
                    or { PersistenceBehaviour: PersistenceBehaviour.WhenUserEditable, IsUserEditable: true } 
                && e.PropertyName == nameof(IInterfaceData<>.Value))
            {
                SerializedValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            _target.PropertyChanged -= TargetPropertyChanged;
        }

        public event EventHandler? SerializedValueChanged;
    }

}