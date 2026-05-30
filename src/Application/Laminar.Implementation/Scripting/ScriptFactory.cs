using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;

namespace Laminar.Implementation.Scripting;

internal class ScriptFactory(IScriptExecutionManager scriptExecutionManager)
    : IScriptFactory
{
    public IScript CreateScript() => new Script(scriptExecutionManager);
}
