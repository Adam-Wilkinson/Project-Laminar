using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Domain.Extensions;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Connections;

internal class Connection(IOutputConnector outputConnector, IInputConnector inputConnector, INodeGraph nodeGraph)
    : IConnection, IEqualityComparer<IConnection>
{
    public IInputConnector InputConnector { get; } = inputConnector;

    public IOutputConnector OutputConnector { get; } = outputConnector;

    public INodeGraph OwningNodeGraph { get; } = nodeGraph;

    public ConnectionData Data => field ??= ComputeData();

    public bool Equals(IConnection? x, IConnection? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.InputConnector.Equals(y.InputConnector) && x.OutputConnector.Equals(y.OutputConnector);
    }

    public int GetHashCode(IConnection obj)
    {
        return HashCode.Combine(obj.InputConnector, obj.OutputConnector);
    }

    public static bool TryCreateFromData(ConnectionData data, INodeGraph nodeGraph, [NotNullWhen(true)] out Connection? connection)
    {
        if (!nodeGraph.TryGetNodeByKey(data.OutputNodeId, out var outputNode) ||
            !nodeGraph.TryGetNodeByKey(data.InputNodeId, out var inputNode)
            || outputNode.Rows[data.OutputNodeRow].OutputConnector is not { } outputConnector
            || inputNode.Rows[data.InputNodeRow].InputConnector is not { } inputConnector)
        
        {
            connection = null;
            return false;
        }

        connection = new Connection(outputConnector, inputConnector, nodeGraph);
        return true;
    }

    private ConnectionData ComputeData()
    {
        var outputNode = OwningNodeGraph.GetParentNode(OutputConnector);
        var outputNodeKey = OwningNodeGraph.GetNodeKey(outputNode);
        var outputNodeRow = outputNode.Rows.FindIndex(x => x.OutputConnector == OutputConnector);
        
        var inputNode = OwningNodeGraph.GetParentNode(InputConnector);
        var inputNodeKey = OwningNodeGraph.GetNodeKey(inputNode);
        var inputNodeRow = inputNode.Rows.FindIndex(x => x.InputConnector == InputConnector);

        if (outputNodeRow == -1 || inputNodeRow == -1)
        {
            throw new InvalidOperationException("Unable to find connectors to create connection data");
        }
        
        return new ConnectionData(outputNodeKey, outputNodeRow, inputNodeKey, inputNodeRow);
    }
}

public record ConnectionData(string OutputNodeId, int OutputNodeRow, string InputNodeId, int InputNodeRow);