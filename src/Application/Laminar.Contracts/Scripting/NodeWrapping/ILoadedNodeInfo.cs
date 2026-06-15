using Laminar.Contracts.Base.PluginLoading;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface ILoadedNodeInfo
{
    public IWrappedNode CreateInstance();
    
    public Type NodeType { get; }
    
    public IRegisteredPlugin Plugin { get; }
}