using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

public interface IOutputConnector<T> : IOutputConnector where T : IOutput
{
    public void Init(T output);

    public T Output { get; }
}
