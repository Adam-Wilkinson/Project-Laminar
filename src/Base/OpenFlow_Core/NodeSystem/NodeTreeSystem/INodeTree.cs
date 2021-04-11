using OpenFlow_Core.NodeSystem.Connection;
using OpenFlow_Core.NodeSystem.Nodes;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenFlow_Core.NodeSystem.NodeTreeSystem
{
    public interface INodeTree
    {
        ReadOnlyObservableCollection<INodeBase> Nodes { get; }

        void AddNode(INodeBase newNode);
        IConnector GetActiveConnector(IConnector interacted);
        IEnumerable<INodeConnection> GetConnections();
        bool TryConnectFields(IConnector field1, IConnector field2);
    }
}