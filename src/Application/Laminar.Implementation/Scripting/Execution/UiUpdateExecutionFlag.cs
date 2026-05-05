using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.Execution;

public static class UiUpdateExecutionFlag
{
    public static readonly ExecutionFlags Value = ExecutionFlags.ReserveNext();

    extension(ExecutionFlags)
    {
        public static ExecutionFlags UiUpdate => Value;
    }

    extension(ExecutionFlags flags)
    {
        public bool IsUiUpdate => flags.HasFlag(Value);
    }
}
