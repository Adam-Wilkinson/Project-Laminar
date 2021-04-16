namespace Laminar_PluginFramework.NodeSystem.Nodes
{
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;

    public interface ITypeConverterNode : IFunctionNode
    {
        IVisualNodeComponent ConvertInput { get; }

        IVisualNodeComponent ConvertOutput { get; }
    }
}
