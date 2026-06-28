using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Scripting;

public interface IScriptingFactory
{
    IScript CreateScript();

    INodeTree CreateNodeTree(IEnumerable<IWrappedNode> nodes, IEnumerable<IConnection> connections);
        
    INodeTree NodeTreeFromPersistentData(IPersistentDictionary persistentDictionary);
}
