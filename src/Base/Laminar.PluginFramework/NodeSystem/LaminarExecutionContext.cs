using System;

namespace Laminar.PluginFramework.NodeSystem;

/// <summary>
/// Context used by nodes to determine what actions should be taken
/// </summary>
public record LaminarExecutionContext(object? ExecutionSource, ExecutionFlags ExecutionFlags, DateTime TimeOfStart);

[Flags]
public enum ExecutionFlags
{
    /// <summary>
    /// This execution was triggered by a trigger node, and should influence the system
    /// </summary>
    Trigger = 1,

    /// <summary>
    /// The UI needs updating
    /// </summary>
    UpdateUI = 2,

    /// <summary>
    /// There has been a change in some values the node depends on
    /// </summary>
    ValuesChanged = 4,
}