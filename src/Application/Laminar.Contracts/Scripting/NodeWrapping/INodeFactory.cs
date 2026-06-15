using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeFactory
{
    IWrappedNode FromPersistentData(IPersistentDictionary persistentDictionary,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null);

    IWrappedNode FromNodeInfo(ILoadedNodeInfo nodeInfo,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null);
}
