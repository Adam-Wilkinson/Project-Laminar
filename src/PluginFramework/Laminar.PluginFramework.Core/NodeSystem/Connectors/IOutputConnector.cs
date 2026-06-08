namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IOutputConnector : IConnector
{
    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags);

    public bool TryConnectTo(IInputConnector connector);

    /// <summary>
    /// Gets a value that indicates whether this output connector could possibly connect to a given input.
    /// This should be a pure function independent of current state, and is simply a first step in the connection process
    /// </summary>
    /// <param name="connector">The potential input connector</param>
    /// <returns>True if this output is capable of connecting to the input, false otherwise</returns>
    public bool CouldConnectTo(IInputConnector connector);
}
