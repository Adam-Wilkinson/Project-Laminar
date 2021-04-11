namespace OpenFlow_PluginFramework.NodeSystem.Nodes
{
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;

    public interface ITypeConverterNode : IFunctionNode
    {
        IVisualNodeComponent ConvertInput { get; }

        IVisualNodeComponent ConvertOutput { get; }
    }
}
