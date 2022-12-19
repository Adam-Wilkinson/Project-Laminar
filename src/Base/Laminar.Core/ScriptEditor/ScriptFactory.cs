using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Execution;
using Laminar.Core.ScriptEditor.Connections;

namespace Laminar.Core.ScriptEditor;

internal class ScriptFactory : IScriptFactory
{
    private readonly IScriptExecutionManager _scriptExecutionManager;
    private readonly IUserActionManager _userActionManager;

    public ScriptFactory(IScriptExecutionManager scriptExecutionManager, IUserActionManager userActionManager)
    {
        _scriptExecutionManager = scriptExecutionManager;
        _userActionManager = userActionManager;
    }

    public IScript CreateScript() => new EditableScript(_scriptExecutionManager, new ConnectionCollection(_userActionManager));
}
