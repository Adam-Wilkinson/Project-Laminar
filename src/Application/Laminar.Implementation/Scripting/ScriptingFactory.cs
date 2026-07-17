using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Scripting;

internal class ScriptingFactory(
    IScriptExecutionManager scriptExecutionManager, 
    IEncodableDataFactory dataFactory,
    INodeFactory nodeFactory,
    ILogger<WritableNodeTree> logger)
    : IScriptingFactory
{
    public IScript CreateScript() 
        => new Script(scriptExecutionManager, dataFactory.GetEncodableData<IPersistentDictionary>(), this);

    public INodeTree CreateNodeTree(IEnumerable<IWrappedNode> nodes, IEnumerable<IConnection> connections, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) 
        => new WritableNodeTree(dataFactory.GetEncodableData<IPersistentDictionary>(), nodeFactory, logger, userChangedValueClient, nodes, connections);

    public INodeTree NodeTreeFromPersistentData(IPersistentDictionary persistentDictionary, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) 
        => new WritableNodeTree(persistentDictionary, nodeFactory, logger, userChangedValueClient);

    public IScript FromPersistentData(IPersistentDictionary encodableData) 
        => new Script(scriptExecutionManager, encodableData, this);
}
