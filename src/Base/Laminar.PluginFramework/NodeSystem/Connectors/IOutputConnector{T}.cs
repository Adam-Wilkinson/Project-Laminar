using Laminar.PluginFramework.NodeSystem.IO;

namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IOutputConnector<T> : IOutputConnector where T : IOutput
{
    public void Init(T output);

    public T Output { get; }
}
