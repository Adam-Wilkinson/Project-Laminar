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
    [ShowInNode] private readonly ManualNodeRow<None, InterfaceData<Slider, double>, IValueOutput<double>> SliderRow =
        Component.ManualOutputRow(
            new InterfaceData<Slider,double> { Definition = new Slider { Min = 0.0, Max = 5.0 }, IsUserEditable = true, Value = 2, Name = "Test Slider Display" },
            NodeIO.ValueOutput("Test Slider Output", 0.0));

    public string NodeName => "Slider Test";

    public void Evaluate()
    {
        SliderRow.Output!.Value = SliderRow.Display.Value;
    }
}
