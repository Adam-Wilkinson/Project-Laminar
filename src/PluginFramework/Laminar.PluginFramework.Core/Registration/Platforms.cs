using System;

namespace Laminar.PluginFramework.Registration;

[Flags]
public enum Platforms
{
    None = 0,
    Windows = 1,
    Mac = 2,
    Linux = 4,
    All = Windows | Mac | Linux,
}
