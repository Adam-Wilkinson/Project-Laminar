using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Contexts;

/// <summary>
/// Context used by nodes to determine what actions should be taken
/// </summary>
public class ExecutionContext
{
    /// <summary>
    /// The reason for the execution. Use this to determine how the system should be affected by the execution (e.g. don't move the mouse during an interface refresh)
    /// </summary>
    public ExecutionReason ExecutionReason { get; init; }

    /// <summary>
    /// The source of the execution. If <see cref="ExecutionReason"/> is <see cref="ExecutionReason.Trigger"/> this contains the GUID of the trigger
    /// </summary>
    public Guid ExecutionSource { get; init; }

    /// <summary>
    /// The time at which the execution started
    /// </summary>
    public DateTime TimeOfStart { get; init; }
}
