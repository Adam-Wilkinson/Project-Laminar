using System;
using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.Maths.Comparisons;

public class Equal : INode
{
    private readonly ValueInput<double> inputOne = new("First Input", 0.0);
    private readonly ValueInput<double> inputTwo = new("First Output", 0.0);
    private readonly ValueOutput<bool> outputField = new("Equal", false);

    public string NodeName => "Equal";

    public void Evaluate()
    {
        outputField.Value = inputOne.Value == inputTwo.Value;
    }
}
