using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IRuntimeHost
{
    public string Name { get; set; }

    public IPluginManager PluginManager { get; }
    
    public IScriptingFactory ScriptingFactory { get; }
    
    public ILoadedNodeManager NodeManager { get; }
}