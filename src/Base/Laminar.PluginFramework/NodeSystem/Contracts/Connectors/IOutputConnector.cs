namespace Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

public interface IOutputConnector : IIOConnector
{
    public bool TryConnectTo(IInputConnector connector);

    public void OnDisconnectedFrom(IInputConnector connector);
}
