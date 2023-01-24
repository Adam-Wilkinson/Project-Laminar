using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeDifference : INode
{
    readonly ValueInput<double> FirstNumber = new("First Number", 0.0);
    readonly ValueInput<double> SecondNumber = new("Second Number", 0.0);
    readonly ValueOutput<double> Difference = new("Difference", 0.0);

    public string NodeName => "Difference";

    public void Evaluate()
    {
        Difference.Value = FirstNumber - SecondNumber;
    }
}
