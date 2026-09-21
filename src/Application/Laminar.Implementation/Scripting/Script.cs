using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Base;
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
    
    private readonly CompositeDisposable _subscriptions;
    private readonly IScriptExecutionInstance _executionInstance;
    private readonly IScriptingFactory _scriptingFactory;
    private readonly IExceptionHandler _exceptionHandler;
    
    private IWritableNodeTree _nodeTree;
    
    public Script(
        IRuntimeHost runtime,
        IUserActionManager userActionManager,
        IPersistentDictionary persistentData,
        IScriptExecutionManager executionManager, 
        IExceptionHandler exceptionHandler,
        IScriptingFactory scriptingFactory)
    {
        _scriptingFactory = scriptingFactory;
        _exceptionHandler = exceptionHandler;
        
        Runtime = runtime;
        Data = persistentData;
        ActionScope = userActionManager.CreateScope(new ScriptActionSimplifier());
        RefreshNodeTree();
        if (_nodeTree is null) throw new InvalidOperationException("NodeTree should not be null here");

        _executionInstance = executionManager.CreateExecutionInstance(NodeTree);
        
        Pan = persistentData[nameof(Pan)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        Zoom = persistentData[nameof(Zoom)].GetValueOrInitialize(1.0);

        _subscriptions = new(
            NodeTree.Nodes.SubscribeForEach(OnNodeAdded, OnNodeRemoved), 
                Runtime.PluginManager.Plugins.SubscribeForEach(OnPluginInstalled, OnPluginRemoved));
    }

    private void OnPluginRemoved(IInstalledPlugin obj) => RefreshNodeTree();

    private void OnPluginInstalled(IInstalledPlugin newPlugin) => RefreshNodeTree();

    private void RefreshNodeTree()
    {
        _nodeTree?.Dispose();
        try
        {
            _nodeTree = (IWritableNodeTree)_scriptingFactory.NodeTreeFromPersistentData(Data[NodeTreeKey]
                .GetOrCreateCollection<IPersistentDictionary>());
            OnPropertyChanged(nameof(NodeTree));
        }
        catch (Exception ex)
        {
            _exceptionHandler.OnException(ex);
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
