using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.NodeWrapping;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Scripting;

internal class NodeGraphDataHandler(
    IEncodableDataFactory dataFactory,
    IExceptionHandler exceptionHandler,
    ILogger<WritableNodeGraph> logger) : INodeGraphDataHandler
{
    public INodeGraph CreateNodeTree(
        IWorkspace hostWorkspace, 
        IEnumerable<INodeContainer> nodes, 
        IEnumerable<IConnection> connections)
        => new WritableNodeGraph(dataFactory.GetEncodableData<IPersistentDictionary>(), hostWorkspace, logger,
            exceptionHandler, nodes, connections);

    public INodeGraph NodeTreeFromPersistentData(
        IWorkspace hostWorkspace, 
        IPersistentDictionary nodeTreeData) 
        => new WritableNodeGraph(nodeTreeData, hostWorkspace, logger, exceptionHandler);

    public void SetPluginVersion(
        IPersistentDictionary nodeTreeData,
        string pluginId,
        SemanticVersion newVersion)
    {
        var nodes = nodeTreeData[WritableNodeGraph.NodeKey].GetOrCreateCollection<IPersistentDictionary>();
        foreach (var dataPoint in nodes.Values)
        {
            if (!dataPoint.GetOrCreateCollection<IPersistentDictionary>()[NodeFactory.PluginVersionKey]
                    .GetValue<SemanticVersion>().TryGetResult(out var versionValue, out _))
                continue;

            versionValue.Value = newVersion;
        }
    }
}