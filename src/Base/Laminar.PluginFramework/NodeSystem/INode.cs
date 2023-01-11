namespace Laminar.PluginFramework.NodeSystem;

public interface INode
{
    string NodeName { get; }

    void Evaluate();
}