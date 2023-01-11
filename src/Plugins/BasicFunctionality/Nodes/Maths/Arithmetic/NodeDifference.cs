using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeDifference : INode
{
    ValueInput<double> FirstNumber = new("First Number", 0.0);

    ValueInput<double> SecondNumber = new("Second Number", 0.0);

    ValueOutput<double> Difference = new("Difference", 0.0);

    public string NodeName => "Difference";

    public void Evaluate()
    {
        Difference.Value = FirstNumber - SecondNumber;
    }
}
