using Laminar.Contracts.Base;
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
    IScriptExecutionManager scriptExecutionManager, 
    IEncodableDataFactory dataFactory,
    IExceptionHandler exceptionHandler,
    ILogger<WritableNodeTree> logger)
    : IScriptingFactory
{
    public IScript CreateScript() 
        => new Script(host, dataFactory.GetEncodableData<IPersistentDictionary>(), scriptExecutionManager, this);

    public IScript FromPersistentData(IPersistentDictionary persistentDictionary) 
        => new Script(host, persistentDictionary, scriptExecutionManager, this);

    public INodeTree CreateNodeTree(IEnumerable<INodeContainer> nodes, IEnumerable<IConnection> connections,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null)
        => new WritableNodeTree(dataFactory.GetEncodableData<IPersistentDictionary>(), host.NodeManager, logger,
            exceptionHandler, nodes, connections);

    public INodeTree NodeTreeFromPersistentData(
        IPersistentDictionary persistentDictionary,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) 
        => new WritableNodeTree(persistentDictionary, host.NodeManager, logger, exceptionHandler);
}
