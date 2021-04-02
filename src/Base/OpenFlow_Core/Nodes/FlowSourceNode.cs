namespace OpenFlow_Core.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using OpenFlow_Core.Nodes.Connectors;
    using OpenFlow_Core.Nodes.VisualNodeComponentDisplays;
    using OpenFlow_PluginFramework;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;

    public class FlowSourceNode : IFlowNode
    {
        private readonly INodeField _sourceField = Constructor.NodeField("Manual Trigger").WithValue<INodeField, Action>("Displayed", null, false).WithFlowOutput();
        private NodeBase _parentNodeBase;

        public FlowSourceNode()
        {
            // this.SetSpecialField(SpecialFieldFlags.FlowOutput, _sourceField);
        }

        public IVisualNodeComponent FlowOutField => _sourceField;

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
                (_parentNodeBase.GetDisplayForComponent(_sourceField).OutputConnector.Value as FlowConnector)?.Activate();
            });
        }
    }
}
