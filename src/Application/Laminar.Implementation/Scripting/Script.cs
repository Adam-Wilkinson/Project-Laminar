using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting;

internal class Script : IScript, IDisposable
{
    private const string NodeTreeKey = "NodeTree";
    
    private readonly IScriptExecutionInstance _executionInstance;
    
    public Script(IPersistentDictionary persistentData, IScriptExecutionManager executionManager, IScriptingFactory scriptingFactory)
    {
        NodeTree = scriptingFactory.NodeTreeFromPersistentData(persistentData[NodeTreeKey]
            .GetOrCreateCollection<IPersistentDictionary>());

        _executionInstance = executionManager.CreateExecutionInstance(NodeTree);
        
        Pan = persistentData[nameof(Pan)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        Zoom = persistentData[nameof(Zoom)].GetValueOrInitialize(1.0);
        Data = persistentData;
    }
    
    public INodeTree NodeTree { get; }
    
    public IObservableValue<Point> Pan { get; }

    public IObservableValue<double> Zoom { get; }

    public IPersistentDictionary Data { get; }

    public void Dispose()
    {
        NodeTree.Dispose();
        _executionInstance.Dispose();
    }
}
