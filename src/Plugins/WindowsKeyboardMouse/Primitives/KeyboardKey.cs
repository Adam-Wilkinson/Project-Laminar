using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private string _asString;

        public KeyboardKey(int virtualKey, KeyModifiers modifiers = KeyModifiers.None)
        {
            HookKey = (Keys)virtualKey;
            StringBuilder builder = new();

            if (modifiers.HasFlag(KeyModifiers.Ctrl) && (Keys)virtualKey is not Keys.LControlKey or Keys.RControlKey)
            {
                builder.Append("Ctrl + ");
                HookKey |= Keys.Control;
            }

            if (modifiers.HasFlag(KeyModifiers.Shift) && (Keys)virtualKey is not Keys.LShiftKey or Keys.RShiftKey)
            {
                builder.Append("Shift + ");
                HookKey |= Keys.Shift;
            }

            if (modifiers.HasFlag(KeyModifiers.Alt) && (Keys)virtualKey is not Keys.LMenu or Keys.RMenu)
            {
                builder.Append("Alt + ");
                HookKey |= Keys.Alt;
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

        public Keys HookKey { get; }

        public override string ToString() => _asString;

        public override bool Equals(object obj)
        {
            return obj is KeyboardKey key && key.HookKey == HookKey;
        }

        public override int GetHashCode()
        {
            return HookKey.GetHashCode();
        }
    }
}
