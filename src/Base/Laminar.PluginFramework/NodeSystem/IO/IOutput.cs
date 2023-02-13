using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.PluginFramework.NodeSystem.IO;

public interface IOutput : INodeIO
{
    public void StartExecution();

    public IOutputConnector Connector { get; }
}