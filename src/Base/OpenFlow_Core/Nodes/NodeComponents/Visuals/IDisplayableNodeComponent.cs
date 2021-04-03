using OpenFlow_Core.Nodes.Connectors;
using OpenFlow_Core.Primitives;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;

namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    public interface IDisplayableNodeComponent : IVisualNodeComponent
    {
        IConnectionManager ConnectionManager { get; }
    }
}