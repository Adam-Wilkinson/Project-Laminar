namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IInputConnector : IConnector
{
    public bool TryConnectTo(IOutputConnector connector);
    
    /// <summary>
    /// Gets a value that indicates whether this input connector could possibly connect to a given output.
    /// This should be a pure function independent of current state, and is simply a first step in the connection process
    /// </summary>
    /// <param name="connector">The potential output connector</param>
    /// <returns>True if this input is capable of connecting to the output, false otherwise</returns>
    public bool CouldConnectTo(IOutputConnector connector);
}
