using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface ILoadedNodeInfo
{
    public INode CreateInstance();
    
    public Type NodeType { get; }

    public string Id { get; }

    public VersionedPluginId PluginId { get; }
}