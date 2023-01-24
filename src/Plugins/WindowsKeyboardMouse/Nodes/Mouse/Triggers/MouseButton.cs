using Laminar.PluginFramework.NodeSystem;

namespace WindowsKeyboardMouse.Nodes.Mouse.Triggers;

public class MouseButton : INode
{
    // private readonly INodeField mouseButtonOutput = Constructor.NodeField("Mouse Button").WithValue<MouseButtons>("Display", true).WithOutput<MouseButtons>();

    public string NodeName => "Mouse Button";

    public void Evaluate()
    {
        // mouseButtonOutput[INodeField.OutputKey] = mouseButtonOutput["Display"];
    }
}
