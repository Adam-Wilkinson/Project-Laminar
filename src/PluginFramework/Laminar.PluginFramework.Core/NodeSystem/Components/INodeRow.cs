using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Components;

public interface INodeRow : INodeComponent
{
    public IInputConnector? InputConnector { get; }

    public IOutputConnector? OutputConnector { get; }

    public IInterfaceData CentralDisplay { get; }
}