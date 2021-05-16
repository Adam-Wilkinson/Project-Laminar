using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPluginBase.Window
{
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

        private class WindowEnumerable : IEnumerable<Window>
        {
            TaskCompletionSource tsc = null;
            bool completed;

            public IEnumerator<Window> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            private class WindowEnumerator : IEnumerator<Window>
            {
                public Window Current => throw new NotImplementedException();

                object IEnumerator.Current => throw new NotImplementedException();

                public void Dispose()
                {
                    throw new NotImplementedException();
                }

                public bool MoveNext()
                {
                    throw new NotImplementedException();
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
