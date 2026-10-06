using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class LoadedNodeInfo<T>(VersionedPluginId pluginId) : ILoadedNodeInfo where T : INode, new()
{
    public INode CreateInstance() => new T();

    public Type NodeType => typeof(T);

    public NodeDescriptor NodeDescriptor { get; } = new(pluginId,
        typeof(T).FullName ?? throw new InvalidOperationException("Node type must have a type name"));
}