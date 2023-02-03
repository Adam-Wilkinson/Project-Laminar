using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeDivide : INode
{
    [ShowInNode] readonly ValueInputRow<double> FirstNumber = new("Numerator", 0.0);
    [ShowInNode] readonly ValueInputRow<double> SecondNumber = new("Denominator", 1.0);
    [ShowInNode] readonly ValueOutputRow<double> ResultNumber = new("Result", 0.0);

    public string NodeName => "Divide";

    public void Evaluate()
    {
        ResultNumber.Value = FirstNumber / SecondNumber;
    }
}
