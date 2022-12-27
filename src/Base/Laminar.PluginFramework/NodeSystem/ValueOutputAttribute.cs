using System;
using System.Diagnostics;
using System.Reflection;
using Laminar.PluginFramework.NodeSystem.Attributes;

namespace Laminar.PluginFramework.NodeSystem;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ValueOutputAttribute<T> : Attribute, IConvertsToNodeRowAttribute
{
    public ValueOutputAttribute(string Name, bool ShowConnector = true, string? TriggerEventName = null)
    {
        this.TriggerEventName = TriggerEventName;
        this.Name = Name;
        this.ShowConnector = ShowConnector;
    }

    public string? TriggerEventName { get; }
    public string Name { get; }
    public bool ShowConnector { get; }

    public NodeRow GenerateNodeRow(PropertyInfo childProperty, object containingObject)
    {
        var propertyGetter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), containingObject, childProperty.GetGetMethod(true)!);
        var propertySetter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), containingObject, childProperty.GetSetMethod(true)!);

        Action<EventHandler> ValueChangedEvent = null;
        if (TriggerEventName is not null && containingObject.GetType().GetEvent(TriggerEventName) is EventInfo triggerEventInfo)
        {
            ValueChangedEvent = (Action<EventHandler>)Delegate.CreateDelegate(typeof(Action<EventHandler>), containingObject, triggerEventInfo.GetAddMethod());
        }

        var output = new AttributeValueOutput<T>(propertyGetter, propertySetter, Name, ValueChangedEvent);
        return new NodeRow(null, output, ShowConnector ? output : null);
    }

    public class AttributeValueOutput<T> : ValueOutput<T>
    {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;

        public AttributeValueOutput(Func<T> getter, Action<T> setter, string name, Action<EventHandler>? SubscribeToChangedAction) : base(name, getter())
        {
            _getter = getter;
            _setter = setter;
            SubscribeToChangedAction?.Invoke((sender, args) => FireExecutionEvent());
        }

        public override T Value
        {
            get => _getter();
            set => _setter?.Invoke(value);
        }
    }
}
