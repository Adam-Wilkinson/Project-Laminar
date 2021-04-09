using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;

namespace OpenFlow_Core.Nodes.Connection
{
    public interface IConnector
    {
        ConnectorType ConnectionType { get; set; }

        bool IsExclusiveConnection { get; }

        INodeConnection ExclusiveConnection { get; }

        IObservableValue<bool> Exists { get; }

        // INodeBase ParentNode { get; set; }

        object Tag { get; set; }

        IObservableValue<string> HexColour { get; }

        void HookupRefreshTriggers(IVisualNodeComponent component);

        void AddConnection(INodeConnection connection);

        void RemoveConnection(INodeConnection connection);

        bool CanConnectTo(IConnector toConnectTo);
    }
}
