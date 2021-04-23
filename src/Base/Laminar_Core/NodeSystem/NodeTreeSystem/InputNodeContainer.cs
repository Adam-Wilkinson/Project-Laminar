using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.Nodes.NodeTypes;
using Laminar_Core.Scripts;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem.NodeTreeSystem
{
    public class InputNodeContainer<T> : NodeContainer<T> where T : INode, new()
    {
        private readonly Dictionary<IAdvancedScriptInstance, object> _instanceValues = new();

        public InputNodeContainer(NodeDependencyAggregate dependencies) : base(dependencies)
        {
        }

        public void SetValue(IAdvancedScriptInstance instance, object value)
        {
            SetFieldValue(instance, FieldList[0] as INodeField, (FieldList[0] as INodeField).GetValue(INodeField.OutputKey), value);
        }

        public object GetValue(IAdvancedScriptInstance instance)
        {
            return GetFieldValue(instance, FieldList[0] as INodeField, (FieldList[0] as INodeField).GetValue(INodeField.OutputKey));
        }

        public override void SetFieldValue(IAdvancedScriptInstance instance, INodeField containingField, ILaminarValue laminarValue, object value)
        {
            if (instance is null)
            {
                base.SetFieldValue(instance, containingField, laminarValue, value);
            }
            else
            {
                _instanceValues[instance] = value;
                Update(instance);
            }
        }

        public override object GetFieldValue(IAdvancedScriptInstance instance, INodeField containingField, ILaminarValue laminarValue)
        {
            if (instance is not null && _instanceValues.TryGetValue(instance, out object value))
            {
                return value;
            }
            else
            {
                return base.GetFieldValue(instance, containingField, laminarValue);
            }
        }
    }
}
