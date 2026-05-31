namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IInputConnector : IConnector
{
    public bool CanConnectTo(IOutputConnector connector);
    
    public bool TryConnectTo(IOutputConnector connector);
}
