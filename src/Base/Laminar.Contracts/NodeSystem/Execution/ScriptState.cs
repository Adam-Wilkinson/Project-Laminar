namespace Laminar.Contracts.NodeSystem.Execution;

/// <summary>
/// Describes the current state of a script execution instance
/// </summary>
public enum ScriptState
{
    /// <summary>
    /// The script is manually disabled and cannot run
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// The script is active but not currently running
    /// </summary>
    Active = 1,

    /// <summary>
    /// The script is currently running and executing code
    /// </summary>
    Running = 2,
}