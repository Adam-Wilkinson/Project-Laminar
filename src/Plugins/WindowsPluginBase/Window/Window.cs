using System;
using System.Runtime.InteropServices;

namespace WindowsPluginBase.Window;

public class Window
{
    public IntPtr hWnd { get; init; }

    public RECT Location
    {
        get => WindowHooks.GetWindowRect(hWnd);
        set => WindowHooks.SetWindowRect(hWnd, value);
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

    [DllImport("USER32.DLL")]
    private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);
}
