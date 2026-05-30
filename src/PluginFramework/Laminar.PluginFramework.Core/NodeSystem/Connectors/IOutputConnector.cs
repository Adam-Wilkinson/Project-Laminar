namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IOutputConnector : IConnector
{
    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags);

    public bool TryConnectTo(IInputConnector connector);

    public bool CanConnectTo(IInputConnector connector);
}
