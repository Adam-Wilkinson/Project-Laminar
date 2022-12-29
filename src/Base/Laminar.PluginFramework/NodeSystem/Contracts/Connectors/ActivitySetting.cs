namespace Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

/// <summary>
/// Options used to specify how an object behaves with activity
/// </summary>
public enum ActivitySetting
{
    /// <summary>
    /// The object is always active and will never be inactive
    /// </summary>
    AlwaysActive = 0,

    /// <summary>
    /// The object is currently active but may become inactive
    /// </summary>
    CurrentlyActive = 1,

    /// <summary>
    /// The object is currently inactive
    /// </summary>
    Inactive = 2,
}