using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeDifference : INode
{
    [ShowInNode] readonly ValueInput<double> FirstNumber = new("First Number", 0.0);
    [ShowInNode] readonly ValueInput<double> SecondNumber = new("Second Number", 0.0);
    [ShowInNode] readonly ValueOutput<double> Difference = new("Difference", 0.0);

    public string NodeName => "Difference";

    public void Evaluate()
    {
        Difference.Value = FirstNumber - SecondNumber;
    }
}
