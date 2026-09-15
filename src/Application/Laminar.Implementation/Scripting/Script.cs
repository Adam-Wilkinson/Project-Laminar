using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Actions;
using Laminar.Implementation.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting;

internal class Script : IScript, INodeHost, IDisposable
{
    private const string NodeTreeKey = "NodeTree";
    
    private readonly IScriptExecutionInstance _executionInstance;
    private readonly IWritableNodeTree _nodeTree;

    private readonly CompositeDisposable _subscriptions;
    
    public Script(
        IRuntimeHost runtime,
        IUserActionManager userActionManager,
        IPersistentDictionary persistentData,
        IScriptExecutionManager executionManager, 
        IScriptingFactory scriptingFactory)
    {
        Runtime = runtime;
        ActionScope = userActionManager.CreateScope(new ScriptActionSimplifier());
        _nodeTree = (IWritableNodeTree)scriptingFactory.NodeTreeFromPersistentData(persistentData[NodeTreeKey]
            .GetOrCreateCollection<IPersistentDictionary>());

        _executionInstance = executionManager.CreateExecutionInstance(_nodeTree);
        
        Pan = persistentData[nameof(Pan)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        Zoom = persistentData[nameof(Zoom)].GetValueOrInitialize(1.0);
        Data = persistentData;

        _subscriptions = new(
                _nodeTree.Nodes.SubscribeForEach(OnNodeAdded, OnNodeRemoved), 
                Runtime.PluginManager.Plugins.SubscribeForEach(OnPluginInstalled));
    }

    private void OnPluginInstalled(IInstalledPlugin newPlugin)
    {
        foreach (var node in _nodeTree.Nodes)
        {
            if (node.Descriptor.Plugin != newPlugin.PluginId ||
                !newPlugin.TryGetNodeInfo(node.Descriptor.NodeName, out var nodeInfo)) continue;
            
            ((NodeContainer)node).OnNodeInfoLoaded(nodeInfo);
        }
    }

    private void OnNodeAdded(INodeContainer obj) => ((NodeContainer)obj).AttachTo(this);

    private void OnNodeRemoved(INodeContainer obj) => ((NodeContainer)obj).DetachFromHost();

    public IRuntimeHost Runtime { get; }
    
    public IUserActionScope ActionScope { get; }
    
    public INodeCollection Nodes => _nodeTree;

    public INodeTree NodeTree => _nodeTree;
    
    public IObservableValue<Point> Pan { get; }

    public IObservableValue<double> Zoom { get; }

    public IPersistentDictionary Data { get; }

    public void Dispose()
    {
        NodeTree.Dispose();
        _executionInstance.Dispose();
        _subscriptions.Dispose();
    }
}
