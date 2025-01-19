using Laminar.PluginFramework;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;
using static Laminar.PluginFramework.LaminarFactory;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;

public partial class SliderTestNode : INode
{
    [ShowInNode] private readonly ManualNodeRow<None, IDisplayValue, IValueOutput<double>> SliderRow =
        Component.ManualOutputRow(
            NodeIO.ValueInput("Test Slider", 2.0, new Slider { Min = 0.0, Max = 5.0 }).DisplayValue,
            NodeIO.ValueOutput("Test Slider Output", 0.0));

    public string NodeName { get; } = "Slider Test";

    public void Evaluate()
    {
        SliderRow.Output.Value = (double)SliderRow.Display.Value;
    }
}
