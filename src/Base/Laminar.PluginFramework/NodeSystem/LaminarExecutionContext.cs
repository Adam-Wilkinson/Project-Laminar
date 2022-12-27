using System;

namespace Laminar.PluginFramework.NodeSystem;

/// <summary>
/// Context used by nodes to determine what actions should be taken
/// </summary>
public readonly record struct LaminarExecutionContext(object? ExecutionSource, ExecutionFlags.ExecutionFlags ExecutionFlags);