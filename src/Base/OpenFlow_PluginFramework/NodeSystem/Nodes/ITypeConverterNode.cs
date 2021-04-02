namespace OpenFlow_PluginFramework.NodeSystem.Nodes
{
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;

    public interface ITypeConverterNode : INode
    {
        IVisualNodeComponent ConvertInput { get; }

        IVisualNodeComponent ConvertOutput { get; }
    }
}
