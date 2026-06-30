using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Scripting.Execution;

namespace Laminar.Implementation.Scripting;

internal class ScriptingFactory(
    IScriptExecutionManager scriptExecutionManager, 
    IEncodableDataFactory dataFactory,
    INodeFactory nodeFactory)
    : IScriptingFactory
{
    public IScript CreateScript() 
        => new Script(scriptExecutionManager, dataFactory.GetEncodableData<IPersistentDictionary>(), this);

    public INodeTree CreateNodeTree(IEnumerable<IWrappedNode> nodes, IEnumerable<IConnection> connections) 
        => new WritableNodeTree(dataFactory.GetEncodableData<IPersistentDictionary>(), nodeFactory, nodes, connections);

    public INodeTree NodeTreeFromPersistentData(IPersistentDictionary persistentDictionary) 
        => new WritableNodeTree(persistentDictionary, nodeFactory);

    public IScript FromPersistentData(IPersistentDictionary encodableData) 
        => new Script(scriptExecutionManager, encodableData, this);
}
