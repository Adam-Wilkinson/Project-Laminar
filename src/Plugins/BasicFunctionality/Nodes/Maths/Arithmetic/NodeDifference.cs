using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeDifference : INode
{
    [ValueInput<double>("First Number")] double FirstNumber { get; set; } = 0.0;
    [ValueInput<double>("Second Number")] double SecondNumber { get; set; } = 0.0;
    [ValueOutput<double>("Difference")] double Difference { get; set; } = 0.0;

    public string NodeName => "Difference";

    public void Evaluate()
    {
        Difference = FirstNumber - SecondNumber;
    }
}
