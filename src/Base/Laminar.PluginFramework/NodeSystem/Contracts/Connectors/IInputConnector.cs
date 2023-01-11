namespace Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

public interface IInputConnector : IIOConnector
{
    public bool TryConnectTo(IOutputConnector connector);

    public void OnDisconnectedFrom(IOutputConnector connector);
}
