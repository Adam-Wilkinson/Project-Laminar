using System;

namespace WindowsKeyboardMouse.Primitives;

[Flags]
public enum KeyModifiers
{
    None = 0,
    Alt = 1,
    Ctrl = 2,
    Shift = 4,
    Meta = 8
}
