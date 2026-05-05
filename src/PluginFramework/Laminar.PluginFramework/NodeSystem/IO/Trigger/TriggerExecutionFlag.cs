namespace Laminar.PluginFramework.NodeSystem.IO.Trigger;

public static class TriggerExecutionFlag
{
    public static readonly ExecutionFlags Value = ExecutionFlags.ReserveNext();

    extension(ExecutionFlags)
    {
        public static ExecutionFlags Trigger => Value;
    }

    extension(ExecutionFlags flags)
    {
        public bool HasTriggerFlag => flags.HasFlag(Value);
    }
}