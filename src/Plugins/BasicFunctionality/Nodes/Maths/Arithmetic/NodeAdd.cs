using System.Collections;
using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;
using static Laminar.PluginFramework.LaminarFactory;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;

public partial class NodeAdd : INode
{
    [ShowInNode] readonly INodeComponentCloner<ValueInputRow<double>> AddInputs = Component.Cloner(() => Component.ValueInput("Add input", 0.0), 1);
    [ShowInNode] readonly ValueOutputRow<double> Output = Component.ValueOutput("Sum", 0.0);

    [Input("This is an input")] int ThisIsATestInput = 0;

    [Input("Another input")] string OooAnotherOne = "Initial value";

    [Input("Wonder what these do")] double TestOne = 3, TestTwo = 5;

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