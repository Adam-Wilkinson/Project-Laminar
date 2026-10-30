using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Contracts.Workspaces;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Scripting;

public interface INodeGraphDataHandler
{
    public INodeGraph CreateNodeTree(
        IWorkspace hostWorkspace, 
        IEnumerable<INodeContainer> nodes, 
        IEnumerable<IConnection> connections);
        
    public INodeGraph NodeTreeFromPersistentData(
        IWorkspace hostWorkspace, 
        IPersistentDictionary nodeTreeData);

    public void SetPluginVersion(
        IPersistentDictionary nodeTreeData, 
        string pluginId, 
        SemanticVersion newVersion);
}