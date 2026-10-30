using Laminar.Contracts.Storage.PersistentData;
using Laminar.Contracts.Workspaces;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeFactory
{
    public INodeContainer FromPersistentData(IPersistentDictionary persistentDictionary, IWorkspace host);

    public INodeContainer FromDescriptor(NodeDescriptor nodeInfo, IWorkspace host);
}
