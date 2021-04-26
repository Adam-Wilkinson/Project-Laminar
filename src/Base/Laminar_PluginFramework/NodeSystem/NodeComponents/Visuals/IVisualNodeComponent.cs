using Laminar_PluginFramework.Primitives;

namespace Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals
{
    public interface IVisualNodeComponent : INodeComponent
    {
        IObservableValue<string> Name { get; }

        IFlow FlowInput { get; }

        IFlow FlowOutput { get; }
    }
}