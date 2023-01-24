using Laminar.PluginFramework.NodeSystem;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Output;

public class IsKeyDown : INode
{
    // private readonly INodeField _keyField = Constructor.NodeField("Key").WithInput<KeyboardKey>().WithOutput<bool>();

    public string NodeName { get; } = "Is Key down";

    public void Evaluate()
    {
        //Debug.WriteLine($"Someone asked if the key {_keyField.GetInput()} is down. It got the value {_keyField.GetInput<KeyboardKey>().IsPressed()}");
        //_keyField[INodeField.OutputKey] = _keyField.GetInput<KeyboardKey>().IsPressed();
    }
}
