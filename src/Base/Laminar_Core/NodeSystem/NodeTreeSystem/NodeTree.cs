using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Laminar_Core.NodeSystem.Connection;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripts;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.NodeSystem.NodeTreeSystem
{
    public class NodeTree : Script, INodeTree
    {
        private readonly List<INodeConnection> _connections = new();
        private readonly ObservableCollection<INodeContainer> _nodes = new();
        private readonly INodeFactory _nodeFactory;
        private readonly INodeConnectionFactory _connectionFactory;

        public NodeTree(ScriptDependencyAggregate deps, INodeFactory nodeFactory, INodeConnectionFactory connectionFactory, INodeTreeInputs inputs)
            : base(deps)
        {
            _nodeFactory = nodeFactory;
            _connectionFactory = connectionFactory;
            Inputs = inputs;
            Nodes = new(_nodes);

            Name.Value = "Advanced Script";

            INodeContainer flowSourceNode = _nodeFactory.Get<ManualTriggerNode>();
            flowSourceNode.MakeLive();
            AddNode(flowSourceNode);
        }

        public INodeTreeInputs Inputs { get; }

        public ReadOnlyObservableCollection<INodeContainer> Nodes { get; }

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

        public void AddNode(INodeContainer newNode)
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
