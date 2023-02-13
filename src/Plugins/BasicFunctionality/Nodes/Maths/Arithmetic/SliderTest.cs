using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;
using static Laminar.PluginFramework.LaminarFactory;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;

public class SliderTest : INode
{
    [ShowInNode] readonly ValueOutputRow<double> Output = Component.ValueOutput("Slider Test", 2.0, isUserEditable: true, editor: new Slider(0, 4));

    public string NodeName { get; } = "Slider Test";

    public void Evaluate()
    {
    }
}
