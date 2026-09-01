using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.Implementation.Scripting;
using Laminar.Implementation.Scripting.NodeWrapping;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Base;

public class RuntimeHost : IRuntimeHost, INotifyPropertyChanged
{
    public RuntimeHost(IServiceProvider serviceProvider)
    {
        NodeManager = ActivatorUtilities.CreateInstance<LoadedNodeManager>(serviceProvider, this);
        PluginManager = ActivatorUtilities.CreateInstance<PluginManager>(serviceProvider, this);
        ScriptingFactory = ActivatorUtilities.CreateInstance<ScriptingFactory>(serviceProvider, this);
    }

    public required string Name { get; set => SetField(ref field, value); }
    
    public IPluginManager PluginManager { get; }
    
    public IScriptingFactory ScriptingFactory { get; }
    
    public ILoadedNodeManager NodeManager { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}