namespace OpenFlow_Core.Nodes.NodeTreeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using OpenFlow_Core.Nodes.Connectors;
    using OpenFlow_Core.Nodes.VisualNodeComponentDisplays;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;

    public class NodeTree
    {
        private readonly Dictionary<IConnector, IConnector> _connections = new();

        public ObservableCollection<NodeBase> Nodes { get; private set; } = new();

        public NodeTree()
        {
            FlowSourceNode flowSourceNode = new();
            NodeBase flowSourceNodeBase = new(flowSourceNode);
            flowSourceNode.SetParentNode(flowSourceNodeBase);
            AddNode(flowSourceNodeBase);
        }

        public bool TryConnectFields(IConnector field1, IConnector field2)
        {
            if (NodeConnection.Construct(field1, field2, out NodeConnection newConnection))
            {
                if (SimpleConnect(newConnection))
                {
                    return true;
                }

                /*
                 * 
                 * This code is for automatic type converting and is not currently implemented
                if (TypeConverter.TryGetConverter(
                    (newConnection.Output as ValueConnector)?.DisplayValue.TypeDefinition.ValueType,
                    (newConnection.Input as ValueConnector)?.DisplayValue.TypeDefinition.ValueType,
                    out Type converterType))
                {
                    NodeBase newNode = new((INode)Activator.CreateInstance(converterType));
                    if (newNode.TryGetSpecialField(SpecialFieldFlags.ConvertInput, out NodeFieldDisplay convertInput) && newNode.TryGetSpecialField(SpecialFieldFlags.ConvertOutput, out NodeFieldDisplay convertOutput) &&
                        NodeConnection.Construct(newConnection.Output, convertInput.InputConnector.Value, out NodeConnection firstConnection) && SimpleConnect(firstConnection) &&
                        NodeConnection.Construct(convertOutput.OutputConnector.Value, newConnection.Input, out NodeConnection secondConnection) && SimpleConnect(secondConnection))
                    {
                        //newNode.X = (field1.Parent.X + field2.Parent.X) / 2;
                        //newNode.Y = (field1.Parent.Y + field2.Parent.Y) / 2;

                        AddNode(newNode);
                    }

                    return true;
                }
                */
            }

            return false;
        }

        public IConnector ConnectionChanged(IConnector interacted)
        {
            if (_connections.TryGetValue(interacted, out IConnector value))
            {
                _connections.Remove(interacted);
                interacted.TryRemoveConnection(value);
                return value;
            }

            return interacted;
        }

        public void AddNode(NodeBase newNode)
        {
            Debug.WriteLine("Node Added");
            Nodes.Add(newNode);
        }

        public IEnumerable<NodeConnection> GetConnections()
        {
            Dictionary<IConnector, IConnector>.Enumerator enumerator = _connections.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (NodeConnection.Construct(enumerator.Current.Key, enumerator.Current.Value, out NodeConnection connection))
                {
                    yield return connection;
                }
            }
        }

        private bool SimpleConnect(NodeConnection connection)
        {
            if (connection.Input.TryAddConnection(connection.Output) || connection.Output.TryAddConnection(connection.Input))
            {
                if (connection.Input.IsExclusiveConnection)
                {
                    _connections[connection.Input] = connection.Output;
                }
                else if (connection.Output.IsExclusiveConnection)
                {
                    _connections[connection.Output] = connection.Input;
                }

                return true;
            }

            return false;
        }
    }
}
