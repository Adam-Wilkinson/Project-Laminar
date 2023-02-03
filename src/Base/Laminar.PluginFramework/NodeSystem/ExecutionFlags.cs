namespace Laminar.PluginFramework.NodeSystem;

public struct ExecutionFlags
{
    private static int HighestIndex = -1;

    private ExecutionFlags(int intValue)
    {
        AsNumber = intValue;
    }

    public int AsNumber { get; }

    public static int ReserveNextFlagValue() => 1 << HighestIndex++;

    public static bool HasFlag(ExecutionFlags flags, int flagValue) => (flags.AsNumber & flagValue) != 0;

    public static ExecutionFlags None => 0;

    public static ExecutionFlags operator &(ExecutionFlags left, ExecutionFlags right) => new(left.AsNumber + right.AsNumber);

    public static implicit operator ExecutionFlags(int numericValue) => new(numericValue);

    public override int GetHashCode()
    {
        return AsNumber.GetHashCode();
    }
}