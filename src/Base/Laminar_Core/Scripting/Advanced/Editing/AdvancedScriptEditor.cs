using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
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
        private bool _isLive;
        private ObservableCollection<INodeContainer> _nodes = new();
        private INodeConnectionFactory _connectionFactory;
        private readonly List<INodeConnection> _connections = new();

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

        public ReadOnlyObservableCollection<INodeContainer> Nodes { get; }

        public void AddNode(INodeContainer newNode)
        {
            _nodes.Add(newNode);
        }

        public void DeleteNode(INodeContainer node)
        {
            _nodes.Remove(node);
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
    }
}
