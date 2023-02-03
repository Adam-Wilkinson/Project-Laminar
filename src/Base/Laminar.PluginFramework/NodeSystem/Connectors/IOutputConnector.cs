namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IOutputConnector : IIOConnector
{
    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags);

    public bool TryConnectTo(IInputConnector connector);

    public void OnDisconnectedFrom(IInputConnector connector);
}
