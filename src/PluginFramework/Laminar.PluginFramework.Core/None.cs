namespace Laminar.PluginFramework;

/// <summary>
/// Stub class used for null generic arguments
/// </summary>
public sealed class None
{
    public static readonly None Instance = new();

    private None()
    {
    }
}
