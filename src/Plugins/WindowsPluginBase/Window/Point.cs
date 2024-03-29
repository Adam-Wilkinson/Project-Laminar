﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPluginBase.Window
{
    public class Point
    {
        public Point(POINT point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public int X { get; set; }

        public int Y { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }
}
