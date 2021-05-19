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
        private readonly Dictionary<Guid, INodeContainer> _nodesByGuid = new();

        private readonly List<INodeContainer> _triggerNodes = new();
        private readonly ObservableCollection<INodeContainer> _nodes = new();
        private readonly INodeConnectionFactory _connectionFactory;
        private readonly List<INodeConnection> _connections = new();
        private bool _isLive;

        public AdvancedScriptEditor(INodeConnectionFactory connectionFactory)
        {
            Nodes = new(_nodes);
            _connectionFactory = connectionFactory;
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

        public IEnumerable<INodeContainer> TriggerNodes => _triggerNodes;

        public ReadOnlyObservableCollection<INodeContainer> Nodes { get; }

        public void AddNode(INodeContainer newNode)
        {
            if (newNode.CanGetCoreNodeOfType(out ITriggerNode _))
            {
                _triggerNodes.Add(newNode);
            }
            _nodes.Add(newNode);
            _nodesByGuid.Add(newNode.Guid, newNode);
        }

        public void DeleteNode(INodeContainer node)
        {
            if (node.CanGetCoreNodeOfType(out ITriggerNode _))
            {
                _triggerNodes.Remove(node);
            }

            _nodes.Remove(node);
            _nodesByGuid.Remove(node.Guid);
        }

        public INodeContainer GetNode(Guid guid)
        {
            return _nodesByGuid[guid];
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

        public IEnumerable<INodeConnection> Connections => _connections;

        public IAdvancedScriptInputs Inputs { get; }

        public bool TryConnectFields(IConnector field1, IConnector field2, out INodeConnection connection)
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
