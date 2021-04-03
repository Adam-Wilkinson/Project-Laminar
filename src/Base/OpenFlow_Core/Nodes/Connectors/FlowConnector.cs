namespace OpenFlow_Core.Nodes.Connectors
{
    using OpenFlow_Core.Nodes;
    using OpenFlow_Core.Nodes.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using System.Diagnostics;

    public class FlowConnector : Connector<FlowConnector>
    {
        public FlowConnector(INodeBase parent,  ConnectionType connectionType)
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

        public static FlowConnector ConnectorCheck(IVisualNodeComponent component, ConnectionType connectionType)
        {
            if (GetFlowFor(component, connectionType))
            {
                return new FlowConnector(null, connectionType);
            }

            return null;
        }

        private static bool GetFlowFor(IVisualNodeComponent component, ConnectionType connectionType) => (connectionType) switch
        {
            ConnectionType.Input => component.GetFlowInput().Value,
            ConnectionType.Output => component.GetFlowOutput().Value,
            _ => false,
        };
    }
}
