using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables;
using Laminar.PluginFramework.NodeSystem;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Scripting;

internal class ScriptingFactory(
    IRuntimeHost host,
    IExecutionManager executionManager,
    IUserActionManager userActionManager,
    IEncodableDataFactory dataFactory,
    IExceptionHandler exceptionHandler,
    ILogger<WritableNodeGraph> logger)
    : IScriptingFactory
{
    public IScript CreateScript() 
        => new Script(host, userActionManager, dataFactory.GetEncodableData<IPersistentDictionary>(), executionManager, exceptionHandler, this);

    public IScript FromPersistentData(IPersistentDictionary persistentDictionary) 
        => new Script(host, userActionManager, persistentDictionary, executionManager, exceptionHandler, this);

    public INodeGraph CreateNodeTree(IEnumerable<INodeContainer> nodes, IEnumerable<IConnection> connections,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null)
        => new WritableNodeGraph(dataFactory.GetEncodableData<IPersistentDictionary>(), host, logger,
            exceptionHandler, nodes, connections);

    public INodeGraph NodeTreeFromPersistentData(
        IPersistentDictionary persistentDictionary,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) 
        => new WritableNodeGraph(persistentDictionary, host, logger, exceptionHandler);
}
