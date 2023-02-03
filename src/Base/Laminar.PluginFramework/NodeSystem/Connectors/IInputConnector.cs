namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IInputConnector : IIOConnector
{
    public bool TryConnectTo(IOutputConnector connector);

    public void OnDisconnectedFrom(IOutputConnector connector);
}
