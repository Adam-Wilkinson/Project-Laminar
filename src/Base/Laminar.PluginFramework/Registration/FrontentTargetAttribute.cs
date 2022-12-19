using System;

namespace Laminar.PluginFramework.Registration;

[AttributeUsage(AttributeTargets.Assembly)]
public class FrontentTargetAttribute : Attribute
{
    public FrontentTargetAttribute(FrontentTarget frontentTarget)
    {
        FrontentTarget = frontentTarget;
    }

    public FrontentTarget FrontentTarget { get; }
}

public enum FrontentTarget
{
    None,
    Avalonia,
}
