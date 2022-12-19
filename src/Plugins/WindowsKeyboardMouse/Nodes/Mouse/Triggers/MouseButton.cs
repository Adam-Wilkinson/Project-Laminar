using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System.Collections.Generic;

namespace WindowsKeyboardMouse.Nodes.Mouse.Triggers;

public class MouseButton : IFunctionNode
{
    // private readonly INodeField mouseButtonOutput = Constructor.NodeField("Mouse Button").WithValue<MouseButtons>("Display", true).WithOutput<MouseButtons>();

    public string NodeName => "Mouse Button";

    public IEnumerable<INodeComponent> Fields
    {
        get
        {
            yield return null;// mouseButtonOutput;
        }
    }

    public void Evaluate()
    {
        // mouseButtonOutput[INodeField.OutputKey] = mouseButtonOutput["Display"];
    }
}
