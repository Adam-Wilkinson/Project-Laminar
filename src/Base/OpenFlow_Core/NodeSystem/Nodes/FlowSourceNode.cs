using System;
using System.Collections.Generic;
using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.Primitives.TypeDefinition;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class FlowSourceNode : IFlowNode
    {
        private readonly INodeField _sourceField = Constructor.NodeField("Manual Trigger").WithValue("Displayed", Constructor.ManualTypeDefinitionManager().WithAcceptedDefinition(Constructor.TypeDefinition<Action>(null)), false).WithFlowOutput();
        // private INodeBase _parentNodeBase;

        public FlowSourceNode()
        {
            _sourceField["Displayed"] = (Action)(() =>
            {
                INodeBase.NodeBases[this].Update();
            });
        }

        public IVisualNodeComponent FlowOutComponent => _sourceField;

        public string NodeName => "Flow Source";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _sourceField;
            }
        }

        public void Evaluate()
        {
        }
    }
}
