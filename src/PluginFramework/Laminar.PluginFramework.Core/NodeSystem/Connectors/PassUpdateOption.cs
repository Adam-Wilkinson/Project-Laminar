namespace Laminar.PluginFramework.NodeSystem.Connectors;

/// <summary>
/// Options used to specify how an update is passed through a connector, and whether that reaction is likely to change
/// </summary>
public enum PassUpdateOption
{
    /// <summary>
    /// The update will always pass through
    /// </summary>
    AlwaysPasses = 0,

    /// <summary>
    /// The update currently passes through but that may change
    /// </summary>
    CurrentlyPasses = 1,

    /// <summary>
    /// The update currently does not pass though but that may change
    /// </summary>
    CurrentlyDoesNotPass = 2,

    /// <summary>
    /// The update will never pass through this connector
    /// </summary>
    NeverPasses = 3,
}