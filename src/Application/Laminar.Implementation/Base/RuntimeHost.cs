using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.Implementation.Scripting;
using Laminar.Implementation.Scripting.NodeWrapping;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Base;

public class RuntimeHost : IRuntimeHost
{
    public RuntimeHost(IServiceProvider serviceProvider)
    {
        NodeManager = ActivatorUtilities.CreateInstance<LoadedNodeManager>(serviceProvider);
        PluginManager = ActivatorUtilities.CreateInstance<PluginManager>(serviceProvider, this);
        ScriptingFactory = ActivatorUtilities.CreateInstance<ScriptingFactory>(serviceProvider, this);
    }

    public IPluginManager PluginManager { get; }
    
    public IScriptingFactory ScriptingFactory { get; }
    
    public ILoadedNodeManager NodeManager { get; }
}