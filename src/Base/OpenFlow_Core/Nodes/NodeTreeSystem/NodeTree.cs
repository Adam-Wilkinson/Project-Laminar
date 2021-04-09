using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenFlow_Core.Nodes.Connection;

namespace OpenFlow_Core.Nodes.NodeTreeSystem
{
    public class NodeTree
    {
        private readonly List<INodeConnection> _connections = new();
        private readonly INodeFactory _nodeFactory = Instance.Factory.GetImplementation<INodeFactory>();
        private readonly INodeConnectionFactory _connectionFactory = Instance.Factory.GetImplementation<INodeConnectionFactory>();
        private readonly ObservableCollection<INodeBase> _nodes = new();

        public NodeTree()
        {
            AddNode(_nodeFactory.Get<FlowSourceNode>());
            Nodes = new(_nodes);
        }

        public ReadOnlyObservableCollection<INodeBase> Nodes { get; }

        public bool TryConnectFields(IConnector field1, IConnector field2)
        {
            if (_connectionFactory.TryConnect(field1, field2, out INodeConnection newConnection))
            {
                _connections.Add(newConnection);
                newConnection.OnBreak += Connection_OnBreak;
                return true;
            }

            return false;
        }

        private void Connection_OnBreak(object sender, EventArgs e)
        {
            _connections.Remove(sender as INodeConnection);
        }

        public IConnector ConnectionChanged(IConnector interacted)
        {
            if (interacted.ExclusiveConnection is not null)
            {
                _connections.Remove(interacted.ExclusiveConnection);
                IConnector opposite = interacted.ExclusiveConnection.Opposite(interacted);
                interacted.ExclusiveConnection.Break();
                return opposite;
            }

            return interacted;
        }

        public void AddNode(INodeBase newNode)
        {
            _nodes.Add(newNode);
        }

        public IEnumerable<INodeConnection> GetConnections() => _connections;
    }
}
