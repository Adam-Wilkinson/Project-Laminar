using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsKeyboardMouse.Primitives;

public class KeyboardKey
{

    public KeyboardKey(int virtualKey, KeyModifiers modifiers = KeyModifiers.None)
    {
    }

    public int VirtualKey { get; }

    public KeyModifiers Modifiers { get; }

    public string AsString { get; }

    public bool IsPressed()
    {
        return false;
    }
}
