using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Actions;

namespace Laminar.Implementation.Scripting;

internal sealed class Script : IScript
{
    private const string NodeTreeKey = "NodeTree";
    
    private readonly IDisposable _subscriptions;
    private readonly IScriptingFactory _scriptingFactory;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IExecutionManager _executionManager;

    private ScriptingContext _context;
    
    public Script(
        IRuntimeHost runtime,
        IUserActionManager userActionManager,
        IPersistentDictionary persistentData,
        IExecutionManager executionManager, 
        IExceptionHandler exceptionHandler,
        IScriptingFactory scriptingFactory)
    {
        _scriptingFactory = scriptingFactory;
        _exceptionHandler = exceptionHandler;
        _executionManager = executionManager;
        
        Runtime = runtime;
        Data = persistentData;
        ActionScope = userActionManager.CreateScope(new ScriptActionSimplifier());
        ReloadContext();
        if (_context is null) throw new InvalidOperationException("NodeTree should not be null here");
        
        Pan = persistentData[nameof(Pan)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        Zoom = persistentData[nameof(Zoom)].GetValueOrInitialize(1.0);

        _subscriptions = Runtime.PluginManager.Plugins.SubscribeForEach(OnPluginsChanged, OnPluginsChanged);
    }
    
    public IRuntimeHost Runtime { get; }
    
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
        _subscriptions.Dispose();
    }
    
    private void OnPluginsChanged(IInstalledPlugin _) => ReloadContext();
    
    private void ReloadContext()
    {
        _context?.Dispose();
        try
        {
            var nodeGraph = (IWritableNodeGraph)_scriptingFactory.NodeTreeFromPersistentData(Data[NodeTreeKey]
                .GetOrCreateCollection<IPersistentDictionary>());
            
            _context = new ScriptingContext(nodeGraph, _executionManager)
            {
                HostScript = this
            };
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NodeGraph)));
        }
        catch (Exception ex)
        {
            _exceptionHandler.OnException(ex);
        }
    }
}
