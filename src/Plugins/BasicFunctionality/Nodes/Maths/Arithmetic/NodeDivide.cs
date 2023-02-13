using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;
using static Laminar.PluginFramework.LaminarFactory;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;

public class NodeDivide : INode
{
    [ShowInNode] readonly ValueInputRow<double> FirstNumber = Component.ValueInput("Numerator", 0.0);
    [ShowInNode] readonly ValueInputRow<double> SecondNumber = Component.ValueInput("Denominator", 1.0);
    [ShowInNode] readonly ValueOutputRow<double> ResultNumber = Component.ValueOutput("Result", 0.0);

    public string NodeName => "Divide";

    public void Evaluate()
    {
        ResultNumber.Value = FirstNumber / SecondNumber;
    }
}
