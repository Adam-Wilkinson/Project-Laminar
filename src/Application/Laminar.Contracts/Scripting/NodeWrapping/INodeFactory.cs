using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeFactory
{
    INodeContainer FromPersistentData(IPersistentDictionary persistentDictionary, IWorkspace host);

    INodeContainer FromDescriptor(NodeDescriptor nodeInfo, IWorkspace host);
}
