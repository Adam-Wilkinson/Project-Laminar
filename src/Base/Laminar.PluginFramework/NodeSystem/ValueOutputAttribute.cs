using System;
using System.Reflection;
using Laminar.PluginFramework.NodeSystem.Attributes;

namespace Laminar.PluginFramework.NodeSystem;
public class ValueOutputAttribute<T> : Attribute, IConvertsToNodeRowAttribute
{
    public ValueOutputAttribute(string Name, bool ShowConnector = true)
    {
        this.Name = Name;
        this.ShowConnector = ShowConnector;
    }

    public string Name { get; }
    public bool ShowConnector { get; }

    public NodeRow GenerateNodeRow(PropertyInfo childProperty, object containingObject)
    {
        var propertyGetter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), containingObject, childProperty.GetGetMethod(true));
        var propertySetter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), containingObject, childProperty.GetSetMethod(true));

        var output = new AttributeValueOutput<T>(propertyGetter, propertySetter, Name);
        return new NodeRow(null, output, ShowConnector ? output : null);
    }

    public class AttributeValueOutput<T> : ValueOutput<T>
    {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;

        public AttributeValueOutput(Func<T> getter, Action<T> setter, string name) : base(name, getter())
        {
            _getter = getter;
            _setter = setter;
        }

        public override T Value => _getter();
    }
}
