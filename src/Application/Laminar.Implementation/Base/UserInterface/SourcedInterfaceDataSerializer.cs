using System.ComponentModel;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Base.UserInterface;

public class SourcedInterfaceDataSerializerFactory : IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SourcedInterfaceData<>))
        {
            return Activator.CreateInstance(typeof(SourcedInterfaceDataSerializer<>).MakeGenericType(type)) as IConditionalSerializer;
        }

        return null;
    }
}

public class SourcedInterfaceDataSerializer<T> : TypeSerializer<SourcedInterfaceData<T>, T> where T : notnull
{
    protected override T SerializeTyped(SourcedInterfaceData<T> toSerialize) => toSerialize.ValueWithoutProvider;

    protected override SourcedInterfaceData<T> DeSerializeTyped(DeserializationRequest<SourcedInterfaceData<T>, T> request)
    {
        if (!request.HasExistingValue)
            throw new InvalidOperationException("Deserializing sourced interface data requires existing value");

        request.ExistingValue!.ValueWithoutProvider = request.Serialized;
        return request.ExistingValue;
    }

    protected override INotifySerializedValueChanged? GetSerializedValueChangedNotifier(SourcedInterfaceData<T> target) 
        => new SerializedValueChangedListener(target);

    private class SerializedValueChangedListener : INotifySerializedValueChanged
    {
        private readonly SourcedInterfaceData<T> _target;
        
        public SerializedValueChangedListener(SourcedInterfaceData<T> target)
        {
            _target = target;
            _target.PropertyChanged += TargetPropertyChanged;
        }

        private void TargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SourcedInterfaceData<>.ValueWithoutProvider))
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