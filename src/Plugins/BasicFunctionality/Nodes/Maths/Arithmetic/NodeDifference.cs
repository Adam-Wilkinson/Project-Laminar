using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeDifference : INode
{
    [ShowInNode] readonly ValueInputRow<double> FirstNumber = new("First Number", 0.0);
    [ShowInNode] readonly ValueInputRow<double> SecondNumber = new("Second Number", 0.0);
    [ShowInNode] readonly ValueOutputRow<double> Difference = new("Difference", 0.0);

    public string NodeName => "Difference";

    public void Evaluate()
    {
        Difference.Value = FirstNumber - SecondNumber;
    }
}
