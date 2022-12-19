using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

public interface IInputConnector<T> : IInputConnector where T : IInput
{
    public void Init(T nodeInput);

    public T Input { get; }
}
