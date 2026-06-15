using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class LoadedNodeInfo<T>(IRegisteredPlugin hostPlugin, INodeFactory nodeFactory) : ILoadedNodeInfo where T : INode, new()
{
    public IWrappedNode CreateInstance() => nodeFactory.WrapNode(new T());

    public Type NodeType => typeof(T);

    public IRegisteredPlugin Plugin => hostPlugin;
}