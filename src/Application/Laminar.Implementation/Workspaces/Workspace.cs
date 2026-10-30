using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Workspaces;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Implementation.Scripting;
using Laminar.Implementation.Scripting.NodeWrapping;
using Laminar.Implementation.Workspaces.PluginLoading;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Workspaces;

public class Workspace : IWorkspace, INotifyPropertyChanged
{
    public Workspace(IServiceProvider serviceProvider, IUserActionManager actionManager)
    {
        NodeManager = ActivatorUtilities.CreateInstance<LoadedNodeManager>(serviceProvider, this);
        PluginManager = ActivatorUtilities.CreateInstance<PluginManager>(serviceProvider, this);
        ScriptingFactory = ActivatorUtilities.CreateInstance<ScriptingFactory>(serviceProvider, this);
        ActionScope = actionManager.CreateScope();
    }

    public required string Name { get; set => SetField(ref field, value); }
    
    public IPluginManager PluginManager { get; }
    
    public IScriptingFactory ScriptingFactory { get; }
    
    public ILoadedNodeManager NodeManager { get; }
    
    public IUserActionScope ActionScope { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}