using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class LoadedNodeInfo<T>(IRegisteredPlugin hostPlugin) : ILoadedNodeInfo where T : INode, new()
{
    public INode CreateInstance() => new T();

    public Type NodeType => typeof(T);

    public IRegisteredPlugin Plugin => hostPlugin;
}