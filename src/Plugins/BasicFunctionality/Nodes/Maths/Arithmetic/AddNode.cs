using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;
using static Laminar.PluginFramework.LaminarFactory;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;

public partial class AddNode : INode
{
    [ShowInNode] readonly INodeComponentCloner<ValueInputRow<double>> AddInputs = Component.Cloner(
        componentGenerator: () => Component.ValueInput("Add input", 0.0), 
        initialComponentCount: 1);

    [Output("Sum")] double Output;

    public string NodeName { get; } = "Add";

    public void Evaluate()
    {
        Output = 0;

        foreach (ValueInputRow<double> row in AddInputs)
        {
            Output += row;
        }
    }
}