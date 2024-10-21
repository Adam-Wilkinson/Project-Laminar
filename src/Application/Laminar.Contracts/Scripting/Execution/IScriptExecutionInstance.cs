using Laminar.Contracts.Notification;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.Execution;

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