using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;

namespace OpenFlow_Core.NodeSystem.Connection
{
    public interface IConnector
    {
        ConnectorType ConnectorType { get; set; }

        bool IsExclusiveConnection { get; }

        INodeConnection ExclusiveConnection { get; }

        IObservableValue<bool> Exists { get; }

        object Tag { get; set; }

        IObservableValue<string> HexColour { get; }

        void Initialize(IVisualNodeComponent component);

        void AddConnection(INodeConnection connection);

        void RemoveConnection(INodeConnection connection);

        bool CanConnectTo(IConnector toConnectTo);

        void Activate();
    }
}
