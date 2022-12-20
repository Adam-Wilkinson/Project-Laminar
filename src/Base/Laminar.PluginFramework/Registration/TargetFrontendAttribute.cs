using System;

namespace Laminar.PluginFramework.Registration;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module)]
public class TargetFrontendAttribute : Attribute
{
    public TargetFrontendAttribute(Frontend frontentTarget)
    {
        FrontEndTarget = frontentTarget;
    }

    public Frontend FrontEndTarget { get; }
}

public enum Frontend
{
    All,
    Avalonia,
}
