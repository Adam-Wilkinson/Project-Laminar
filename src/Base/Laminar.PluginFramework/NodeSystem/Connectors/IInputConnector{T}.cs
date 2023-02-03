using Laminar.PluginFramework.NodeSystem.IO;

namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IInputConnector<T> : IInputConnector where T : IInput
{
    public void Init(T nodeInput);

    public T Input { get; }
}
