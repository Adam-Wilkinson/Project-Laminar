using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPluginBase.Window
{
    public class AllWindowsLayout
    {
        public static AllWindowsLayout GetCurrentLayout()
        {
            AllWindowsLayout output = new();
            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (!IsWindowVisible(hWnd)) return true;

                string windowTitle = WindowHooks.GetWindowTitle(hWnd);
                if (windowTitle is null or "") return true;

                output.WindowPositions[hWnd] = new WindowLayout { Position = WindowHooks.GetWindowRect(hWnd), WindowToPrecede = WindowHooks.GetNextWindow(hWnd) };

                return true;
            }, 0);
            return output;
        }

        public static void SetCurrentLayout(AllWindowsLayout newLayout)
        {
            foreach (var kvp in newLayout.WindowPositions)
            {
                WindowHooks.SetWindowRect(kvp.Key, kvp.Value.Position, kvp.Value.WindowToPrecede);
            }
            WindowHooks.WindowArrangementManuallyChanged();
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        public Dictionary<IntPtr, WindowLayout> WindowPositions { get; } = new();
    }
}
