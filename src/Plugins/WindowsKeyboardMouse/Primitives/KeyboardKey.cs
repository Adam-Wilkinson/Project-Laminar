using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsHook;

namespace WindowsKeyboardMouse.Primitives
{
    public class KeyboardKey
    {
        private static readonly Dictionary<int, string> UserfriendlyNames = new()
        {
            { 0xA0, "Left Shift" },
            { 0xA1, "Right Shift" },
            { 0xA2, "Left control" },
            { 0xA3, "Right control" },
            { 0xA4, "Left Alt" },
            { 0xA5, "Right Alt" },
        };

        private readonly string _asString;

        public KeyboardKey(int virtualKey, KeyModifiers modifiers = KeyModifiers.None)
        {
            VirtualKey = virtualKey; 
            HookKey = (Keys)virtualKey;
            StringBuilder builder = new();

            if (modifiers.HasFlag(KeyModifiers.Ctrl))
            {
                HookKey |= Keys.Control;

                if ((Keys)virtualKey is not Keys.LControlKey or Keys.RControlKey)
                {
                    builder.Append("Ctrl + ");
                }
            }

            if (modifiers.HasFlag(KeyModifiers.Shift))
            {
                HookKey |= Keys.Shift;

                if ((Keys) virtualKey is not Keys.LShiftKey or Keys.RShiftKey)
                {
                    builder.Append("Shift + ");
                }
            }

            if (modifiers.HasFlag(KeyModifiers.Alt))
            {
                HookKey |= Keys.Alt;

                if ((Keys)virtualKey is not Keys.LMenu or Keys.RMenu)
                {
                    builder.Append("Alt + ");
                }
            }

            if (UserfriendlyNames.TryGetValue(virtualKey, out string friendlyName))
            {
                builder.Append(friendlyName);
            }
            else
            {
                builder.Append((Keys)virtualKey);
            }

            _asString = builder.ToString();
        }

        private int VirtualKey { get; }

        public Keys HookKey { get; }

        public bool IsPressed()
        {
            return KeyIsDown(VirtualKey) &&
                (HookKey.HasFlag(Keys.Shift) == (KeyIsDown((int)Keys.LShiftKey) | KeyIsDown((int)Keys.RShiftKey))) &&
                (HookKey.HasFlag(Keys.Control) == (KeyIsDown((int)Keys.LControlKey) | KeyIsDown((int)Keys.RControlKey))) &&
                (HookKey.HasFlag(Keys.Alt) == (KeyIsDown((int)Keys.LMenu) | KeyIsDown((int)Keys.RMenu)));
        }

        public override string ToString() => _asString;

        public override bool Equals(object obj)
        {
            return obj is KeyboardKey key && key.HookKey == HookKey;
        }

        public override int GetHashCode()
        {
            return HookKey.GetHashCode();
        }

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int vKey);

        private static bool KeyIsDown(int vk)
        {
            short keyState = GetKeyState(vk);
            return (keyState & 0x8000) == 0x8000;
        }
    }
}
