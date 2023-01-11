using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Implementation.Scripting.Connections;

namespace Laminar.Implementation.Scripting;

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
