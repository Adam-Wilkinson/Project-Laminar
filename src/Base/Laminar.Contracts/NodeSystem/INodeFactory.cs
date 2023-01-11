using Laminar.Contracts.Primitives;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.NodeSystem;

public interface INodeFactory
{
    INodeWrapper WrapNode<T>(INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) where T : INode, new();

    INodeWrapper WrapNode<T>(T node, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) where T : INode, new();

    INodeWrapper CloneNode<T>(INodeWrapper node, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) where T : INode, new();
}
