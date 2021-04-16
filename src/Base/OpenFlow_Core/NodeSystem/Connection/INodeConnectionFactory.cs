namespace Laminar_Core.NodeSystem.Connection
{
    public interface INodeConnectionFactory
    {
        bool TryConnect(IConnector inputConnector, IConnector outputConnector, out INodeConnection outputConnection);
    }
}
