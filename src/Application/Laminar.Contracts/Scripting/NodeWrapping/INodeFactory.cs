using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeFactory
{
    IWrappedNode FromPersistentData(IPersistentDictionary persistentDictionary, ILoadedNodeManager loadedNodeManage);

    IWrappedNode FromNodeInfo(ILoadedNodeInfo nodeInfo, ILoadedNodeManager loadedNodeManager);
}
