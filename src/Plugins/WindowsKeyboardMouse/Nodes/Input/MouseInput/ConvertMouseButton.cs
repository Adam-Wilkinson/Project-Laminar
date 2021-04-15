using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsHook;

namespace WindowsKeyboardMouse.Nodes.Input.MouseInput
{
    static class ConvertMouseButton
    {
        public static MouseButtonEnum Convert(MouseButtons input) => input switch
        {
            MouseButtons.Left => MouseButtonEnum.LeftButton,
            MouseButtons.Middle => MouseButtonEnum.MiddleButton,
            MouseButtons.Right => MouseButtonEnum.RightButton,
            _ => MouseButtonEnum.None,
        };
    }
}
