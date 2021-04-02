using OpenFlow_Core.Nodes.Connectors;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;

namespace OpenFlow_Core.Nodes.VisualNodeComponentDisplays
{
    public interface IVisualNodeComponentDisplay
    {
        public IOpacity Opacity { get; }

        public IObservableValue<IConnector> InputConnector { get; }

        public IObservableValue<IConnector> OutputConnector { get; }
    }
}
