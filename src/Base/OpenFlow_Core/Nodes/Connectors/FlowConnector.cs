namespace OpenFlow_Core.Nodes.Connectors
{
    using OpenFlow_Core.Nodes;
    using OpenFlow_Core.Nodes.VisualNodeComponentDisplays;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using System.Diagnostics;

    public class FlowConnector : Connector<FlowConnector>
    {
        public FlowConnector(NodeBase parent,  ConnectionType connectionType)
            : base(parent, connectionType)
        {
        }

        public override bool IsExclusiveConnection => ConnectionType == ConnectionType.Output;

        public override string ColourHex => "#800080";

        public void Activate()
        {
            Debug.WriteLine("Flowingggg");
            ParentNode.DeepUpdate();

            FlowConnector NodeFlowOutput = ParentNode.GetFlowOutDisplayConnector();

            NodeFlowOutput?.TypedExclusiveConnection?.Activate();
        }
    }
}
