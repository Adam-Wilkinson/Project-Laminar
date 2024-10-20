namespace Laminar.PluginFramework.NodeSystem.IO.Trigger;

public static class TriggerExecutionFlag
{
    public static readonly int Value = ExecutionFlags.ReserveNextFlagValue();

    public static bool HasTriggerFlag(this ExecutionFlags flags) => ExecutionFlags.HasFlag(flags, Value);
}