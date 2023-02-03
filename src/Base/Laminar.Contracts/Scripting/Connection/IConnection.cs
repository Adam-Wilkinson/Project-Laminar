using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Connection;

public interface IConnection
{
    public event EventHandler OnBroken;

    public IInputConnector InputConnector { get; }

    public IOutputConnector OutputConnector { get; }

    public void Break();
}
