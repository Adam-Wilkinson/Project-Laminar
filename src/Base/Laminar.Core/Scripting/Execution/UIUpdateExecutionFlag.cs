using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Implementation.Scripting.Execution;

public static class UIUpdateExecutionFlag
{
    public static readonly int Value = ExecutionFlags.ReserveNextFlagValue();

    public static bool HasUIUpdateFlag(this ExecutionFlags executionFlags) => ExecutionFlags.HasFlag(executionFlags, Value);
}
