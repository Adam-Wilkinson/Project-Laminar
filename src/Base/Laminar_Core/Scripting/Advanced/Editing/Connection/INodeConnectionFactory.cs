namespace Laminar_Core.Scripting.Advanced.Editing.Connection
{
    public interface INodeConnectionFactory
    {
        bool TryConnect(IConnector inputConnector, IConnector outputConnector, out INodeConnection outputConnection);
    }
}
