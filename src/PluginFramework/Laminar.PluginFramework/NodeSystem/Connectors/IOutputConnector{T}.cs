using Laminar.PluginFramework.NodeSystem.IO;

namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IOutputConnector<out T> : IOutputConnector where T : IOutput
{
    public T Output { get; }
}
