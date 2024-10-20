using System;

namespace Laminar.PluginFramework.Registration;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module)]
public class HasFrontendDependencyAttribute : Attribute
{
    public HasFrontendDependencyAttribute(FrontendDependency frontentTarget)
    {
        FrontendDependency = frontentTarget;
    }

    public FrontendDependency FrontendDependency { get; }
}

public enum FrontendDependency
{
    None,
    Avalonia,
}
