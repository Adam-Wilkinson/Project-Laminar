using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Contracts.Workspaces;
using Laminar.Domain.Observables;
using Laminar.PluginFramework.NodeSystem;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Scripting;

internal class ScriptingFactory(
    IWorkspace host,
    IExecutionManager executionManager,
    IEncodableDataFactory dataFactory,
    INodeGraphDataHandler nodeGraphDataHandler,
    IExceptionHandler exceptionHandler)
    : IScriptingFactory
{
    public IScript CreateScript() 
        => new Script(host, dataFactory.GetEncodableData<IPersistentDictionary>(), executionManager, exceptionHandler, nodeGraphDataHandler);

    public IScript FromPersistentData(IPersistentDictionary persistentDictionary) 
        => new Script(host, persistentDictionary, executionManager, exceptionHandler, nodeGraphDataHandler);
}
