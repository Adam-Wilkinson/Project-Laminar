using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPluginBase.Window
{
    public class WindowLayout
    {
        public RECT Position { get; init; }

        public IntPtr WindowToPrecede { get; init; }
    }
}
