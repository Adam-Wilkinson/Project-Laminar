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
    public IScript CreateScript() => new Script(scriptExecutionManager,
        new WritableNodeTree(dataFactory.GetEncodableData<IPersistentDictionary>(), nodeFactory));

    public INodeTree CreateNodeTree(IEnumerable<IWrappedNode> nodes, IEnumerable<IConnection> connections)
    {
        return new WritableNodeTree(dataFactory.GetEncodableData<IPersistentDictionary>(), nodeFactory, nodes, connections);
    }

    public INodeTree NodeTreeFromPersistentData(IPersistentDictionary persistentDictionary)
    {
        return new WritableNodeTree(persistentDictionary, nodeFactory);
    }
}
