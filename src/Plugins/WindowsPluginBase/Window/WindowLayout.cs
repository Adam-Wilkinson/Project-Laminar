using System;

namespace WindowsPluginBase.Window;

public class WindowLayout
{
    public RECT Position { get; init; }

    public IntPtr WindowToPrecede { get; init; }
}
