using Laminar.Contracts.Base.PluginLoading;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface ILoadedNodeInfo
{
    public INode CreateInstance();
    
    public Type NodeType { get; }
    
    public IRegisteredPlugin Plugin { get; }
}