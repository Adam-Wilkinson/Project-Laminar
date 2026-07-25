using System.ComponentModel;
using Avalonia.Controls;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.Serializers;

public class ClassesSerializer : TypeSerializer<Classes, string>
{
    protected override string SerializeTyped(Classes toSerialize) => string.Join(' ', toSerialize);

    protected override Classes DeSerializeTyped(DeserializationRequest<Classes, string> request)
        => Classes.Parse(request.Serialized);

    protected override INotifySerializedValueChanged? GetSerializedValueChangedNotifier(Classes target)
        => new ClassesChangedNotifier(target);
    
    private class ClassesChangedNotifier : INotifySerializedValueChanged
    {
        private readonly Classes _target;
        
        public ClassesChangedNotifier(Classes target)
        {
            _target = target;
            _target.PropertyChanged += ClassesChanged;
        }
        
        public void Dispose()
        {
            _target.PropertyChanged -= ClassesChanged;
        }

        private void ClassesChanged(object? sender, PropertyChangedEventArgs e)
        {
            SerializedValueChanged?.Invoke(sender, EventArgs.Empty);
        }

        public event EventHandler? SerializedValueChanged;
    }
}