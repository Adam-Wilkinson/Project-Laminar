using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeFactory
{
    IWrappedNode WrapNode<T>(INotificationClient<LaminarExecutionContext>? userChangedValueClient = null) where T : INode, new();

    IWrappedNode CreateMatchingNode(IWrappedNode node, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null);

    IWrappedNode WrapNode(INode node, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null);

    IWrappedNode FromPersistentData(IPersistentDictionary persistentDictionary,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null);

    IWrappedNode FromNodeInfo(ILoadedNodeInfo nodeInfo,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null);
}
