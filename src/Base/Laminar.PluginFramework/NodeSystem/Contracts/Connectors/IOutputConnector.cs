namespace Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

public interface IOutputConnector : IIOConnector
{
    public PassUpdateOption PassUpdate(ExecutionFlags.ExecutionFlags executionFlags);

    public bool TryConnectTo(IInputConnector connector);

    public void OnDisconnectedFrom(IInputConnector connector);
}
