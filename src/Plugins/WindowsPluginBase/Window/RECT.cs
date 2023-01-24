using System.Runtime.InteropServices;

namespace WindowsPluginBase.Window;

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}
