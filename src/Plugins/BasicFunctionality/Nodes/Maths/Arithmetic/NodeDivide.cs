using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeDivide : INode
{
    public string NodeName => "Divide";

    ValueInput<double> FirstNumber = new("Numerator", 0.0);
    ValueInput<double> SecondNumber = new("Denominator", 1.0);
    ValueOutput<double> ResultNumber = new("Result", 0.0);

    public void Evaluate()
    {
        ResultNumber.Value = FirstNumber / SecondNumber;
    }
}
