using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Scripting;

internal class Script : IScript
{
    private const string NodeTreeKey = "NodeTree";
    
    private readonly IWritableNodeTree _writableNodeTree;

    public Script(IScriptExecutionManager executionManager, IPersistentDictionary persistentData, IScriptingFactory scriptingFactory)
    {
        _writableNodeTree =
            (IWritableNodeTree)scriptingFactory.NodeTreeFromPersistentData(persistentData[NodeTreeKey]
                .GetOrCreateCollection<IPersistentDictionary>());
        ExecutionInstance = executionManager.CreateExecutionInstance(WritableNodeTree);
        Data = persistentData;
    }
    
    public INodeTree WritableNodeTree => _writableNodeTree;

    public ScriptState State => ExecutionInstance.State;

    public IScriptExecutionInstance ExecutionInstance { get; }
    
    public IPersistentDictionary Data { get; }
}
