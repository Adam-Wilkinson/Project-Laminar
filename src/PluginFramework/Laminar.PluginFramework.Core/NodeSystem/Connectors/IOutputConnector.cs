namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IOutputConnector : IConnector
{
    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags);
    
    public void OnConnectedTo(IInputConnector input);

    public void OnDisconnectedFrom(IInputConnector input);

    public bool CanConnectTo(IInputConnector input);
}
