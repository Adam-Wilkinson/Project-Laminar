using System;

namespace Laminar_PluginFramework.Primitives;

[Flags]
public enum Platforms
{
    None = 0,
    Windows = 0b001,
    Mac = 0b010,
    Linux = 0b100,
    All = Windows | Mac | Linux,
}
