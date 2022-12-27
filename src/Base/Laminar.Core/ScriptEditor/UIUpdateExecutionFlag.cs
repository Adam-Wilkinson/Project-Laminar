using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Core.ScriptEditor;

public static class UIUpdateExecutionFlag
{
    public static readonly int Value = ExecutionFlags.ReserveNextFlagValue();

    public static bool HasUIUpdateFlag(this ExecutionFlags executionFlags) => ExecutionFlags.HasFlag(executionFlags, Value);
}
