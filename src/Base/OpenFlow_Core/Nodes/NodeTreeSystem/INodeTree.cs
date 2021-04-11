using OpenFlow_Core.Nodes.Connection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenFlow_Core.Nodes.NodeTreeSystem
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