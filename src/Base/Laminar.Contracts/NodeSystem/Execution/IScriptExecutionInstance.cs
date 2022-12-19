using Laminar.Contracts.Primitives;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.NodeSystem.Execution;

public interface IScriptExecutionInstance : INotificationClient<LaminarExecutionContext>
{
    /// <summary>
    /// The current state of the script
    /// </summary>
    public ScriptState State { get; }

    /// <summary>
    /// The script is open in a UI and must update its interface elements
    /// </summary>
    public bool IsShownInUI { get; set; }
}