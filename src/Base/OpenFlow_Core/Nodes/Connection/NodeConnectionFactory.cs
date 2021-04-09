namespace OpenFlow_Core.Nodes.Connection
{
    public class NodeConnectionFactory : INodeConnectionFactory
    {
        public bool TryConnect(IConnector connectorOne, IConnector connectorTwo, out INodeConnection connection)
        {
            if (connectorOne.ConnectionType is ConnectionType.Output && connectorTwo.ConnectionType is ConnectionType.Input)
            {
                return TryMakeConnector(connectorOne, connectorTwo, out connection);
            }

            if (connectorTwo.ConnectionType is ConnectionType.Input && connectorTwo.ConnectionType is ConnectionType.Output)
            {
                return TryMakeConnector(connectorTwo, connectorOne, out connection);
            }

            connection = default;
            return false;
        }

        private static bool TryMakeConnector(IConnector outputConnector, IConnector inputConnector, out INodeConnection successfulConnection)
        {
            if (outputConnector.CanConnectTo(inputConnector))
            {
                INodeConnection connection = new NodeConnection(outputConnector, inputConnector);
                outputConnector.AddConnection(connection);
                inputConnector.AddConnection(connection);
                successfulConnection = connection;
                return true;
            }

            successfulConnection = default;
            return false;
        }
    }
}
