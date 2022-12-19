namespace Laminar_Core.Scripting.Advanced.Editing.Connection
{
    public class NodeConnectionFactory : INodeConnectionFactory
    {
        public bool TryConnect(IConnector connectorOne, IConnector connectorTwo, out INodeConnection connection)
        {
            if (connectorOne.ConnectorType is ConnectorType.Output && connectorTwo.ConnectorType is ConnectorType.Input)
            {
                return TryMakeConnector(connectorOne, connectorTwo, out connection);
            }

            if (connectorTwo.ConnectorType is ConnectorType.Input && connectorTwo.ConnectorType is ConnectorType.Output)
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
