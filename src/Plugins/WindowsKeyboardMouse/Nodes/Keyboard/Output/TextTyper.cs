using System.Runtime.InteropServices;
using Laminar.PluginFramework.NodeSystem;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Output;

public class TextTyper : INode
{
    // private readonly INodeField textField = Constructor.NodeField("Text to type").WithInput<string>();

    public string NodeName { get; } = "Text Typer";

    [DllImport("user32.dll")]
    static extern short VkKeyScan(char ch);

    public void Evaluate()
    {
        /*
        foreach (char character in textField.GetInput<string>())
        {
            short keyNumber = VkKeyScan(character);
            Keys key = (Keys)(((keyNumber & 0xFF00) << 8) | (keyNumber & 0xFF));
            if ((key & Keys.Shift) != Keys.None)
            {
                KeyPresser.PressVirtualKey(unchecked((byte)Keys.LShiftKey), 0);
            }
            KeyPresser.PressVirtualKey((byte)key, 0);
            if ((key & Keys.Shift) != Keys.None)
            {
                KeyPresser.PressVirtualKey(unchecked((byte)Keys.LShiftKey), KeyPresser.KEYEVENTF_KEYUP);
            }
        }
        */
    }
}
