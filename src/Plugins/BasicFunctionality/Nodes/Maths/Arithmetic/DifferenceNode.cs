using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;

public partial class DifferenceNode : INode
{
    [Input("First Number")] double FirstNumber;
    [Input("Second Number")] double SecondNumber;
    [Output("Difference")] double Difference;

    public string NodeName => "Difference";

    public void Evaluate()
    {
        Difference = FirstNumber - SecondNumber;
    }
}
