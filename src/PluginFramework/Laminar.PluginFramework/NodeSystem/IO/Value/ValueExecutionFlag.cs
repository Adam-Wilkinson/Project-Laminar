namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public static class ValueExecutionFlag
{
    public static readonly int Value = ExecutionFlags.ReserveNextFlagValue();

    public static bool HasValueFlag(this ExecutionFlags flags) => ExecutionFlags.HasFlag(flags, Value);
}