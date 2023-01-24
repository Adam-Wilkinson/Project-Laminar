using System.Runtime.InteropServices;

namespace WindowsPluginBase.Window;

[StructLayout(LayoutKind.Sequential)]
public class MONITORINFO
{
    public uint cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public uint dwFlags;
}
