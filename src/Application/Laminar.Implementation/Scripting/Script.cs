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
using Laminar.Implementation.Scripting.UserActions;

namespace Laminar.Implementation.Scripting;

internal sealed class Script : IScript
{
    private const string NodeTreeKey = "NodeTree";
    
    private readonly IScriptingFactory _scriptingFactory;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IExecutionManager _executionManager;
    private readonly IDisposable _pluginsChangedSubscription;
    
    private ScriptingContext _context;
    
    public Script(
        IRuntimeHost runtime,
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
        ActionScope = runtime.ActionScope.CreateChild(new ScriptActionSimplifier());
        _context = CreateContext();
        
        Pan = persistentData[nameof(Pan)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        Zoom = persistentData[nameof(Zoom)].GetValueOrInitialize(1.0);

        _pluginsChangedSubscription =
            Runtime.PluginManager.UserInstalledPlugins.SubscribeForEach(OnPluginsChanged, OnPluginsChanged);
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
        _pluginsChangedSubscription.Dispose();
    }
    
    private void OnPluginsChanged(string _) => ReloadContext();
    
    private void ReloadContext()
    {
        _context.Dispose();
        _context = CreateContext();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NodeGraph)));
    }

    private ScriptingContext CreateContext()
    {
        try
        {
            var nodeGraph = (IWritableNodeGraph)_scriptingFactory.NodeTreeFromPersistentData(
                Data[NodeTreeKey].GetOrCreateCollection<IPersistentDictionary>());
            
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
