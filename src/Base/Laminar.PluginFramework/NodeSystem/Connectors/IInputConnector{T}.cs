using Laminar.PluginFramework.NodeSystem.IO;

namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IInputConnector<T> : IInputConnector where T : IInput
{
    public T Input { get; }
}
