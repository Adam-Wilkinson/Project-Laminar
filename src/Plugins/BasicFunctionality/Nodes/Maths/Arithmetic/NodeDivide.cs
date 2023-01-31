using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeDivide : INode
{
    [ShowInNode] readonly ValueInput<double> FirstNumber = new("Numerator", 0.0);
    [ShowInNode] readonly ValueInput<double> SecondNumber = new("Denominator", 1.0);
    [ShowInNode] readonly ValueOutput<double> ResultNumber = new("Result", 0.0);

    public string NodeName => "Divide";

    public void Evaluate()
    {
        ResultNumber.Value = FirstNumber / SecondNumber;
    }
}
