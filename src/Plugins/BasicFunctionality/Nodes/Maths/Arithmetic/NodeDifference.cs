using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;
using static Laminar.PluginFramework.LaminarFactory;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public partial class NodeDifference : INode
{
    [ShowInNode] readonly ValueInputRow<double> FirstNumber = Component.ValueInput("First Number", 0.0);
    [ShowInNode] readonly ValueInputRow<double> SecondNumber = Component.ValueInput("Second Number", 0.0);
    [ShowInNode] readonly ValueOutputRow<double> Difference = Component.ValueOutput("Difference", 0.0);

    public string NodeName => "Difference";

    public void Evaluate()
    {
        Difference.Value = FirstNumber - SecondNumber;
    }
}
