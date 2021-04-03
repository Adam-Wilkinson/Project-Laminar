using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;

namespace OpenFlow_PluginFramework.Primitives
{
    public interface INodeDecoratorFactory
    {
        INodeDecorator GetDecorator(NodeDecoratorType type);
    }
}