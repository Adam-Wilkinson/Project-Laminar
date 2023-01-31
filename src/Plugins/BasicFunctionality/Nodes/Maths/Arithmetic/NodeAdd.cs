using System.Collections;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeAdd : INode
{
    [ShowInNode] readonly INodeRowCloner<ValueInput<double>> AddInputs = Component.RowCloner(() => new ValueInput<double>("Add input", 0.0), 1);
    [ShowInNode] readonly ValueOutput<double> Output = new ValueOutput<double>("Sum", 0.0);

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
