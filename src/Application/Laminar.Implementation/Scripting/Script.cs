using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Contracts.Workspaces;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.UserActions;

namespace Laminar.Implementation.Scripting;

internal sealed class Script : IScript
{
    private const string NodeTreeKey = "NodeTree";
    
    private readonly INodeGraphDataHandler _nodeGraphDataHandler;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IExecutionManager _executionManager;
    private readonly IPersistentDictionary _nodeTreeData;
    
    private ScriptingContext _context;
    
    public Script(
        IWorkspace runtime,
        IPersistentDictionary persistentData,
        IExecutionManager executionManager, 
        IExceptionHandler exceptionHandler,
        INodeGraphDataHandler nodeGraphDataHandler)
    {
        _nodeGraphDataHandler = nodeGraphDataHandler;
        _exceptionHandler = exceptionHandler;
        _executionManager = executionManager;
        
        Host = runtime;
        Data = persistentData;
        _nodeTreeData = Data[NodeTreeKey].GetOrCreateCollection<IPersistentDictionary>();
        ActionScope = runtime.ActionScope.CreateChild(new ScriptActionSimplifier());
        _context = CreateContext() ?? throw new InvalidOperationException();
        
        Pan = persistentData[nameof(Pan)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        Zoom = persistentData[nameof(Zoom)].GetValueOrInitialize(1.0);

        Host.PluginManager.PluginChanged += OnPluginChanged;
    }

    public IWorkspace Host { get; }
    
    public IUserActionScope ActionScope { get; }

    public INodeGraph NodeGraph => _context.NodeGraph;
    
    public IObservableValue<Point> Pan { get; }

    public IObservableValue<double> Zoom { get; }

    public IPersistentDictionary Data { get; }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        NodeGraph.Dispose();
        _context.Dispose();
        Host.PluginManager.PluginChanged -= OnPluginChanged;
    }
    
    private void OnPluginChanged(object? sender, PluginChangedEventArgs e)
    {
        _context.Dispose();

        if (e.ChangeType == PluginChangedType.VersionChanged)
        {
            var pluginVersion = Host.PluginManager.GetPluginVersion(e.PluginId)
                ?? throw new InvalidOperationException("Plugin version changed event fired, but it does not have a new version");
            
            _nodeGraphDataHandler.SetPluginVersion(_nodeTreeData, e.PluginId, pluginVersion);
        }
        
        _context = CreateContext();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NodeGraph)));   
    }

    private ScriptingContext CreateContext()
    {
        try
        {
            var nodeGraph = (IWritableNodeGraph)_nodeGraphDataHandler.NodeTreeFromPersistentData(Host, _nodeTreeData);
            
            return new ScriptingContext(nodeGraph, _executionManager)
            {
                HostScript = this
            };
            
        }
        catch (Exception ex)
        {
            _exceptionHandler.OnException(ex);
            return _context;
        }
    }
}
