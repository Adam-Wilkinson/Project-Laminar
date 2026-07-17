using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting;

public interface IScriptingFactory : IDecodingFactory<IScript, IPersistentDictionary>
{
    IScript CreateScript();

    INodeTree CreateNodeTree(IEnumerable<IWrappedNode> nodes, IEnumerable<IConnection> connections, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null);
        
    INodeTree NodeTreeFromPersistentData(IPersistentDictionary persistentDictionary, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null);
}
