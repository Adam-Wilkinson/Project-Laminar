namespace Laminar.PluginFramework.NodeSystem;

public readonly struct ExecutionFlags
{
    public static readonly ExecutionFlags None = ReserveNext();
    
    private static int _highestIndex = -1;
    
    private ExecutionFlags(int intValue)
    {
        AsNumber = intValue;
    }

    public int AsNumber { get; }

    public static ExecutionFlags ReserveNext() => new(1 << _highestIndex++);

    public bool HasFlag(ExecutionFlags potentialFlags) => (AsNumber & potentialFlags.AsNumber) != 0;
    
    public static bool HasFlag(ExecutionFlags flags, int flagValue) => (flags.AsNumber & flagValue) != 0;

    public static ExecutionFlags operator &(ExecutionFlags left, ExecutionFlags right) => new(left.AsNumber + right.AsNumber);
    
    public override int GetHashCode()
    {
        return AsNumber.GetHashCode();
    }
}