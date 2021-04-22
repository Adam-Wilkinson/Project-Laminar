using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.Nodes.NodeTypes;
using Laminar_Core.Scripts;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem.NodeTreeSystem
{
    public class InputNodeContainer : NodeContainer<InputNode>
    {
        private readonly Dictionary<IAdvancedScriptInstance, object> _instanceValues = new();

        public InputNodeContainer(NodeDependencyAggregate dependencies) : base(dependencies)
        {
        }

        public override void Update(IAdvancedScriptInstance instance)
        {
            if (_instanceValues.TryGetValue(instance, out object value))
            {
                (FieldList[0] as INodeField).SetOutput(value);
                GetContainer(FieldList[0] as IVisualNodeComponent).OutputConnector.Activate(instance, Connection.PropagationDirection.Forwards);
            }
            else
            {
                // (FieldList[0] as INodeField)
            }
        }
    }
}
