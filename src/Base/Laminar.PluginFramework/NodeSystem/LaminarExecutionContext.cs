using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.PluginFramework.NodeSystem;

/// <summary>
/// Context used by nodes to determine what actions should be taken
/// </summary>
public readonly record struct LaminarExecutionContext(IIOConnector ExecutionSource, ExecutionFlags ExecutionFlags);