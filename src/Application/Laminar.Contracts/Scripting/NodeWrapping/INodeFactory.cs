using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeFactory
{
    public INodeContainer FromPersistentData(IPersistentDictionary persistentDictionary, IWorkspace host);

    public INodeContainer FromDescriptor(NodeDescriptor nodeInfo, IWorkspace host);
}
