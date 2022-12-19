using Laminar.PluginFramework.NodeSystem;
using Laminar_PluginFramework.NodeSystem.Nodes;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeDifference : IFunctionNode
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
