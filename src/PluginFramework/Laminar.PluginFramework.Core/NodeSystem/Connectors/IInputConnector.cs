namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IInputConnector : IConnector
{
    public void OnConnectedTo(IOutputConnector output);

    public void OnDisconnectedFrom(IOutputConnector output);

    public bool CanConnectTo(IOutputConnector output);
}
