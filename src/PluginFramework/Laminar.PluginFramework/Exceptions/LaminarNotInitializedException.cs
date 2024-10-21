using System;

namespace Laminar.PluginFramework.Exceptions;

public class LaminarNotInitializedException : Exception
{
    public LaminarNotInitializedException() : base("Laminar plugin framework has not yet been initialized") { }
}
