using Laminar.Contracts.Notification;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeFactory
{
    IWrappedNode WrapNode<T>(INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) where T : INode, new();

    IWrappedNode WrapNode<T>(T node, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) where T : INode, new();

    IWrappedNode CloneNode<T>(IWrappedNode node, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) where T : INode, new();
}
