namespace OpenFlow_Core.Nodes.Connection
{
    public interface INodeConnectionFactory
    {
        bool TryConnect(IConnector inputConnector, IConnector outputConnector, out INodeConnection outputConnection);
    }
}
