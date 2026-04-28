using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Laminar.Avalonia.ToolSystem;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.Serializers;

public class ClassesSerializer : TypeSerializer<Classes, string>, INotifyingConditionalSerializer
{
    protected override string SerializeTyped(Classes toSerialize) => string.Join(' ', toSerialize);

    protected override Classes DeSerializeTyped(DeserializationRequest<Classes, string> request)
        => Classes.Parse(request.Serialized);

    public INotifySerializedValueChanged GetSerializedValueChangedNotifier(object target)
        => new ClassesChangedNotifier((Classes)target);
    
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