using OpenFlow_PluginFramework.Primitives;

namespace OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals
{
    public interface IVisualNodeComponent : INodeComponent
    {
        IObservableValue<string> Name { get; }
    }
}