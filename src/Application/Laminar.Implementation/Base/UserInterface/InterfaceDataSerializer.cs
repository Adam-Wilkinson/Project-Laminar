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
    public override Type SerializedType => serializer.GetSerializedType(typeof(T));
    
    protected override object SerializeOverride(IInterfaceData<T> toSerialize)
    {
        var persistentValue = toSerialize is IPersistenceOverrideInterfaceData<T> persistenceOverride
            ? persistenceOverride.PersistentValue
            : toSerialize.Value;
        
        return serializer.SerializeObject(persistentValue);
    }

    protected override IInterfaceData<T> DeSerializeOverride(DeserializationRequest request)
    {
        if (request.ExistingInstance is not IInterfaceData<T> existingInstance)
            throw new InvalidOperationException("Deserializing interface data requires existing value");

        var existingValue = existingInstance is IPersistenceOverrideInterfaceData<T> persistenceOverride
            ? persistenceOverride.PersistentValue
            : existingInstance.Value;
            
        existingInstance.SetValue(serializer.DeserializeObject(request with
        {
            TargetType = typeof(T), 
            ExistingInstance = existingValue,
        }));
        
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
            if ((e.PropertyName == nameof(IInterfaceData<>.Value) && _target.IsUserEditable) || e.PropertyName == nameof(IPersistenceOverrideInterfaceData<>.PersistentValue))
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