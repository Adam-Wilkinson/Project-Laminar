using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.PluginFramework.NodeSystem.IO;

public interface IInput : INodeIO
{
    public IInputConnector Connector { get; }
}