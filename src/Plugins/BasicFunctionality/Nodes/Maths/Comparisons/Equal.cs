using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace BasicFunctionality.Nodes.Maths.Comparisons;

public class Equal : INode
{
    //[ShowInNode] readonly ValueInputRow<double> inputOne = new("First Input", 0.0);
    //[ShowInNode] readonly ValueInputRow<double> inputTwo = new("First Output", 0.0);
    //[ShowInNode] readonly ValueOutputRow<bool> outputField = new("Equal", false);

    public string NodeName => "Equal";

    public void Evaluate()
    {
        // outputField.Value = inputOne.Value == inputTwo.Value;
    }
}
