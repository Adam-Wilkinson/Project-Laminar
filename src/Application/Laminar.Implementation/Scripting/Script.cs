using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Value;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting;

internal class Script : IScript
{
    private const string NodeTreeKey = "NodeTree";
    
    private readonly IWritableNodeTree _writableNodeTree;

    public Script(IScriptExecutionManager executionManager, IPersistentDictionary persistentData, IScriptingFactory scriptingFactory)
    {
        _writableNodeTree = (IWritableNodeTree)scriptingFactory.NodeTreeFromPersistentData(
                persistentData[NodeTreeKey].GetOrCreateCollection<IPersistentDictionary>(), this);
        
        ExecutionInstance = executionManager.CreateExecutionInstance(WritableNodeTree);
        
        Pan = persistentData[nameof(Pan)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        Zoom = persistentData[nameof(Zoom)].GetValueOrInitialize(1.0);
        
        Data = persistentData;
    }
    
    public INodeTree WritableNodeTree => _writableNodeTree;

    public ScriptState State => ExecutionInstance.State;
    
    public IObservableValue<Point> Pan { get; }
    
    public IObservableValue<double> Zoom { get; }

    public IScriptExecutionInstance ExecutionInstance { get; }
    
    public IPersistentDictionary Data { get; }
    
    public void TriggerNotification(LaminarExecutionContext context)
    {
        ExecutionInstance?.TriggerNotification(context);
    }
}
