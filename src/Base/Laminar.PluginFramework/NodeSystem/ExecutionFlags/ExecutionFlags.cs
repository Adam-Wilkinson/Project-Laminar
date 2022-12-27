using System;
using System.Formats.Tar;
using System.Numerics;

namespace Laminar.PluginFramework.NodeSystem.ExecutionFlags;

public struct ExecutionFlags
{
    private static int HighestIndex = -1;

    private int _asNumber;

    private ExecutionFlags(int intValue)
    {
        _asNumber = intValue;
    }

    public void AddFlag(int value) => _asNumber += value;

    public static int ReserveNextFlagValue() => 1 << HighestIndex++;

    public static bool HasFlag(ExecutionFlags flags, int flagValue) => (flags._asNumber & flagValue) != 0;

    public static ExecutionFlags operator &(ExecutionFlags left, ExecutionFlags right) => new(left._asNumber + right._asNumber);

    public static implicit operator ExecutionFlags(int numericValue) => new(numericValue);
}