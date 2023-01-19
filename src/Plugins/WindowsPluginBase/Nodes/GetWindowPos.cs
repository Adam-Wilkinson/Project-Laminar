using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes;

public class GetWindowPos : INode
{
    // private readonly INodeField _mainField = Constructor.NodeField("Window").WithInput<Window.Window>().WithOutput<Rectangle>();

    public string NodeName { get; } = "Get Window Position";

    public void Evaluate()
    {
        // _mainField.GetOutput<Rectangle>().Rect = _mainField.GetInput<Window.Window>().Location;
    }
}
