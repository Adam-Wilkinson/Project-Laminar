using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Implementation.Scripting.Connections;
using Laminar.Implementation.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting;

internal class ScriptFactory(IScriptExecutionManager scriptExecutionManager, IUserActionManager userActionManager)
    : IScriptFactory
{
    public IScript CreateScript() 
        => new EditableScript(scriptExecutionManager, new ConnectionCollection(userActionManager), new NodeCollection());
}
