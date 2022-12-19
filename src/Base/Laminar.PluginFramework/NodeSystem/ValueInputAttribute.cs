using System;
using System.Reflection;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.PluginFramework.NodeSystem;

[AttributeUsage(AttributeTargets.Property)]
public class ValueInputAttribute<T> : Attribute, IConvertsToNodeRowAttribute
{
    public ValueInputAttribute(string Name, bool ShowConnector = true)
    {
        this.Name = Name;
        this.ShowConnector = ShowConnector;
    }

    public string Name { get; }

    public bool ShowConnector { get; }

    public NodeRow GenerateNodeRow(PropertyInfo childProperty, object containingObject)
    {
        var test = childProperty.GetGetMethod(true);
        var propertyGetter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), containingObject, childProperty.GetGetMethod(true));
        var propertySetter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), containingObject, childProperty.GetSetMethod(true));

        var input = new AttributeValueInput<T>(propertyGetter, propertySetter, Name);
        return new NodeRow(ShowConnector ? input : null, input, null);
    }

    public class AttributeValueInput<T> : ValueInput<T>
    {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;

        public AttributeValueInput(Func<T> getter, Action<T> setter, string name) : base(name, getter())
        {
            _getter = getter;
            _setter = setter;
        }

        public override Action? PreEvaluateAction => SetPropertyValue;

        private void SetPropertyValue() => _setter(Value);
    }
}
