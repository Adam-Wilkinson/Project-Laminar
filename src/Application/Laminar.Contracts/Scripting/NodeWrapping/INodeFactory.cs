using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeFactory
{
    INodeContainer FromPersistentData(IPersistentDictionary persistentDictionary, IRuntimeHost host);

    INodeContainer FromDescriptor(NodeDescriptor nodeInfo, IRuntimeHost host);
}
