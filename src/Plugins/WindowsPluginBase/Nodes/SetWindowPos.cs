using Laminar.PluginFramework.NodeSystem;

namespace WindowsPluginBase.Nodes;

public class SetWindowPos : INode
{
    //private readonly INodeField _windowField = Constructor.NodeField("Window").WithInput<Window.Window>();
    //private readonly INodeField _position = Constructor.NodeField("Position").WithInput<Rectangle>();

    public string NodeName { get; } = "Set Window Position";

    public void Evaluate()
    {
        // _windowField.GetInput<Window.Window>().Location = _position.GetInput<Rectangle>().Rect;
    }
}
