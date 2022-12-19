using Laminar.Contracts.NodeSystem;
using Laminar.Domain.ValueObjects;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Scripting.Advanced.Editing
{
    public class AdvancedScriptEditor : IAdvancedScriptEditor
    {
        private readonly Dictionary<Identifier<INodeWrapper>, INodeWrapper> _nodesByGuid = new();

        private readonly List<INodeWrapper> _triggerNodes = new();
        private readonly ObservableCollection<INodeWrapper> _nodes = new();
        private readonly INodeConnectionFactory _connectionFactory;
        private readonly List<INodeConnection> _connections = new();
        private bool _isLive;

        public AdvancedScriptEditor(INodeConnectionFactory connectionFactory, IAdvancedScriptInputs inputs)
        {
            Nodes = new(_nodes);
            _connectionFactory = connectionFactory;
            Inputs = inputs;
        }

        public bool IsLive
        {
            get => _isLive;
            set
            {
                if (_isLive != value)
                {
                    _isLive = value;
                    if (_isLive)
                    {
                        foreach (INodeContainer node in _nodes)
                        {
                            node.IsLive = true;
                        }
                    }
                    else
                    {
                        foreach (INodeContainer node in _nodes)
                        {
                            node.IsLive = false;
                        }
                    }
                }
            }
        }

        public IEnumerable<INodeWrapper> TriggerNodes => _triggerNodes;

        public ReadOnlyObservableCollection<INodeWrapper> Nodes { get; }

        public void AddNode(INodeWrapper newNode)
        {
            _nodes.Add(newNode);
            _nodesByGuid.Add(newNode.Id, newNode);
        }

        public void DeleteNode(INodeWrapper node)
        {
            _nodes.Remove(node);
            _nodesByGuid.Remove(node.Id);
        }

        public INodeWrapper GetNode(Identifier<INodeWrapper> guid)
        {
            return _nodesByGuid[guid];
        }

        public Connection.IConnector GetActiveConnector(Connection.IConnector interacted)
        {
            if (interacted.ExclusiveConnection is not null)
            {
                _connections.Remove(interacted.ExclusiveConnection);
                Connection.IConnector opposite = interacted.ExclusiveConnection.Opposite(interacted);
                interacted.ExclusiveConnection.Break();
                return opposite;
            }

            return interacted;
        }

        public IEnumerable<INodeConnection> Connections => _connections;

        public IAdvancedScriptInputs Inputs { get; }

        public bool TryConnectFields(Connection.IConnector field1, Connection.IConnector field2, out INodeConnection connection)
        {
            if (_connectionFactory.TryConnect(field1, field2, out INodeConnection newConnection))
            {
                _connections.Add(newConnection);
                newConnection.OnBreak += Connection_OnBreak;
                connection = newConnection;
                return true;
            }

            connection = default;
            return false;
        }

        private void Connection_OnBreak(object sender, EventArgs e)
        {
            _connections.Remove(sender as INodeConnection);
        }
    }
}
