using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Scripting.Advanced.Editing
{
    public class AdvancedScript : IAdvancedScript
    {
        private readonly List<INodeConnection> _connections = new();
        private readonly ObservableCollection<INodeContainer> _nodes = new();
        private readonly INodeConnectionFactory _connectionFactory;
        private bool _editorIsLive;

        public AdvancedScript(IObservableValue<string> name, INodeConnectionFactory connectionFactory, IAdvancedScriptInputs inputs)
        {
            _connectionFactory = connectionFactory;
            Inputs = inputs;
            Nodes = new(_nodes);

            Name = name;
            Name.Value = "Advanced Script";
        }

        public bool EditorIsLive
        {
            get => _editorIsLive;
            set
            {
                if (_editorIsLive != value)
                {
                    _editorIsLive = value;
                    if (_editorIsLive)
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

        public IObservableValue<string> Name { get; }

        public IAdvancedScriptInputs Inputs { get; }

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

        public void DeleteNode(INodeContainer node)
        {
            _nodes.Remove(node);
        }
    }
}
