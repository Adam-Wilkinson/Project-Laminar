using Laminar_PluginFramework.Primitives;

namespace Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals
{
    public interface IVisualNodeComponent : INodeComponent
    {
        int IndexInParent { get; set; }

        IObservableValue<string> Name { get; }

        IFlow FlowInput { get; }

        IFlow FlowOutput { get; }
    }
}