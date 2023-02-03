using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeAdd : INode
{
    [ShowInNode] readonly INodeComponentCloner<ValueInputRow<double>> AddInputs = Component.Cloner(() => new ValueInputRow<double>("Add input", 0.0), 1);
    [ShowInNode] readonly ValueOutputRow<double> Output = new("Sum", 0.0);

    public string NodeName { get; } = "Add";

    public void Evaluate()
    {
        double total = 0;

        foreach (var row in AddInputs)
        {
            total += row;
        }

        Output.Value = total;
    }
}
