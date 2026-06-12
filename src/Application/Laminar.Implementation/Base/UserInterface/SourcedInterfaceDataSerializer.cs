using System.ComponentModel;
using Laminar.PluginFramework.Serialization;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Base.UserInterface;

public class SourcedInterfaceDataSerializerFactory : IConditionalSerializerFactory
{
    public IConditionalSerializer? TryCreateSerializerFor(Type type)
    {
        if (GetSourcedInterfaceDataType(type) is { } sourcedInterfaceDataType)
        {
            return Activator.CreateInstance(typeof(SourcedInterfaceDataSerializer<>).MakeGenericType(sourcedInterfaceDataType.GetGenericArguments()[0])) as IConditionalSerializer;
        }

        return null;
    }

    private static Type? GetSourcedInterfaceDataType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ISourcedInterfaceData<>))
        {
            return type;
        }

        if (type.IsAssignableTo(typeof(IInterfaceData))
            && type.GetInterfaces().FirstOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISourcedInterfaceData<>))
                is { } sourcedInterfaceDataType)
        {
            return sourcedInterfaceDataType;
        }

        return null;
    }
}

public class SourcedInterfaceDataSerializer<T> : TypeSerializer<ISourcedInterfaceData<T>, T> where T : notnull
{
    protected override T SerializeTyped(ISourcedInterfaceData<T> toSerialize) => ((SourcedInterfaceData<T>)toSerialize).ValueWithoutProvider;

    protected override ISourcedInterfaceData<T> DeSerializeTyped(DeserializationRequest<ISourcedInterfaceData<T>, T> request)
    {
        if (!request.HasExistingValue || request.ExistingValue is not SourcedInterfaceData<T> existingValue)
            throw new InvalidOperationException("Deserializing sourced interface data requires existing value");

        existingValue.ValueWithoutProvider = request.Serialized;
        return existingValue;
    }

    protected override INotifySerializedValueChanged? GetSerializedValueChangedNotifier(ISourcedInterfaceData<T> target) 
        => new SerializedValueChangedListener((SourcedInterfaceData<T>)target);

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