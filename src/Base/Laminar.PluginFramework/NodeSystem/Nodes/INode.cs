namespace Laminar_PluginFramework.NodeSystem.Nodes
{
    public interface INode
    {
        string NodeName { get; }

        void Evaluate();
    }
}