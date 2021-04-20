using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Laminar_Core.NodeSystem.Connection;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.NodeSystem.NodeTreeSystem
{
    public class NodeTree : INodeTree
    {
        private readonly List<INodeConnection> _connections = new();
        private readonly ObservableCollection<INodeBase> _nodes = new();
        private readonly INodeFactory _nodeFactory;
        private readonly INodeConnectionFactory _connectionFactory;

        public NodeTree(IObservableValue<string> treeName, INodeFactory nodeFactory, INodeConnectionFactory connectionFactory, INodeTreeInputs inputs)
        {
            _nodeFactory = nodeFactory;
            _connectionFactory = connectionFactory;
            Inputs = inputs;
            Nodes = new(_nodes);

            Name = treeName;
            Name.Value = "Advanced Script";

            Inputs.ParentTree = this;
            INodeBase flowSourceNode = _nodeFactory.Get<ManualTriggerNode>();
            flowSourceNode.MakeLive();
            AddNode(flowSourceNode);
        }

        public INodeTreeInputs Inputs { get; }

        public ReadOnlyObservableCollection<INodeBase> Nodes { get; }
        public IObservableValue<string> Name { get; }

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

        public IConnector GetActiveConnector(IConnector interacted)
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

        private void Connection_OnBreak(object sender, EventArgs e)
        {
            _connections.Remove(sender as INodeConnection);
        }
    }
}
