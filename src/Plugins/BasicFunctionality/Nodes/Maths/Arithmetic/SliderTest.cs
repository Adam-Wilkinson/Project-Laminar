using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.UserInterfaces;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;

public class SliderTest : INode
{
    public readonly ValueInput<double> _sliderTest = new("Slider Test", 2.0) { Editor = new Slider(0, 4) };

    public string NodeName { get; } = "Slider Test";

    public void Evaluate()
    {
    }
}
