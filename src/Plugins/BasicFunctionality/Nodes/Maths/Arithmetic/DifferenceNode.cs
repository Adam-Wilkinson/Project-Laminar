using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;

public partial class DifferenceNode : INode
{
    [Input("First Number")] double FirstNumber = 1.0;
    [Input("Second Number")] double SecondNumber = 1.0;
    [Output("Difference")] double Difference;

    public string NodeName => "Difference";

    public void Evaluate()
    {
        Difference = FirstNumber - SecondNumber;
    }
}