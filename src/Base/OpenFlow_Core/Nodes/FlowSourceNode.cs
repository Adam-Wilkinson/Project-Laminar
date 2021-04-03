namespace OpenFlow_Core.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using OpenFlow_Core.Nodes.Connectors;
    using OpenFlow_PluginFramework;
    using OpenFlow_PluginFramework.Primitives.TypeDefinition;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_Core.Nodes.NodeComponents.Visuals;

    public class FlowSourceNode : IFlowNode
    {
        private readonly INodeField _sourceField = Constructor.NodeField("Manual Trigger").WithValue("Displayed", Constructor.ManualTypeDefinitionManager().WithAcceptedDefinition(Constructor.TypeDefinition<Action>(null)), false).WithFlowOutput();
        private NodeBase _parentNodeBase;

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

        public void SetParentNode(NodeBase parent)
        {
            _parentNodeBase = parent;
            _sourceField["Displayed"] = (Action)(() =>
            {
                _parentNodeBase.GetFlowOutDisplayConnector()?.Activate();
            });
        }
    }
}
