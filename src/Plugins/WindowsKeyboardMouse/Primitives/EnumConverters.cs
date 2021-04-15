using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsHook;

namespace WindowsKeyboardMouse.Primitives
{
    static class EnumConverters
    {
        public static MouseButtons MouseButton(MouseButtonEnum input) => input switch
        {
            MouseButtonEnum.LeftButton => MouseButtons.Left,
            MouseButtonEnum.MiddleButton => MouseButtons.Middle,
            MouseButtonEnum.RightButton => MouseButtons.Right,
            _ => MouseButtons.None,
        };

        public static Keys KeyboardButton(KeyboardButtonEnum input)
        {
            return (Keys)((int)input);
        }
    }
}
