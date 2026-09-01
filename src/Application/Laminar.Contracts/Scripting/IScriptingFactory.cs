using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting;

public interface IScriptingFactory : IDecodingFactory<IScript, IPersistentDictionary>
{
    IScript CreateScript();

    INodeTree CreateNodeTree(IEnumerable<INodeContainer> nodes, IEnumerable<IConnection> connections, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null);
        
    INodeTree NodeTreeFromPersistentData(IPersistentDictionary persistentDictionary, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null);
}
