using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;
using static Laminar.PluginFramework.LaminarFactory;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;

public class NodeAdd : INode
{
    [ShowInNode] readonly INodeComponentCloner<ValueInputRow<double>> AddInputs = Component.Cloner(() => Component.ValueInput("Add input", 0.0), 1);
    [ShowInNode] readonly ValueOutputRow<double> Output = Component.ValueOutput("Sum", 0.0);

    [Input("Test Input Called")] int ThisIsATestInput = 0;

    public string NodeName { get; } = "Add";

    public void Evaluate()
    {
        ThisIsATestInput = 4;
        double total = 0;

        foreach (var row in AddInputs)
        {
            total += row;
        }

        Output.Value = total;
    }
}
