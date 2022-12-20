using System;

namespace Laminar.PluginFramework.Registration;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module)]
public class TargetFrontendAttribute : Attribute
{
    public TargetFrontendAttribute(Frontent frontentTarget)
    {
        FrontentTarget = frontentTarget;
    }

    public Frontent FrontentTarget { get; }
}

public enum Frontent
{
    All,
    Avalonia,
}
