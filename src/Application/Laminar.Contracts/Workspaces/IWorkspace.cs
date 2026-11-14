using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Workspaces.PluginLoading;

namespace Laminar.Contracts.Workspaces;

public interface IWorkspace
{
    public string Name { get; set; }

    public IPluginManager PluginManager { get; }
    
    public IScriptingFactory ScriptingFactory { get; }
    
    public ILoadedNodeManager NodeManager { get; }

    public IFileSystemRootFolder RootFolder { get; }

    public IUserActionScope ActionScope { get; }
}