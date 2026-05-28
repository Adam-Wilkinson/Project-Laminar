namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public static class ValueExecutionFlag
{
    public static readonly ExecutionFlags Value = ExecutionFlags.ReserveNext();

    extension(ExecutionFlags)
    {
        public static ExecutionFlags ValueChanged => Value;
    }

    extension(ExecutionFlags flags)
    {
        public bool HasValueFlag => flags.HasFlag(Value);
    }
}