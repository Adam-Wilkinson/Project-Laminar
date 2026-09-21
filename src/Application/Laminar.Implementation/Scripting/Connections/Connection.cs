using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Domain.Extensions;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Connections;

internal class Connection(IOutputConnector outputConnector, IInputConnector inputConnector, INodeTree nodeTree)
    : IConnection, IEqualityComparer<IConnection>
{
    public IInputConnector InputConnector { get; } = inputConnector;

    public IOutputConnector OutputConnector { get; } = outputConnector;

    public INodeTree OwningNodeTree { get; } = nodeTree;

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

    public static bool TryCreateFromData(ConnectionData data, INodeTree nodeTree, [NotNullWhen(true)] out Connection? connection)
    {
        if (!nodeTree.TryGetNodeByKey(data.OutputNodeId, out var outputNode) ||
            !nodeTree.TryGetNodeByKey(data.InputNodeId, out var inputNode)
            || outputNode.Rows[data.OutputNodeRow].OutputConnector is not { } outputConnector
            || inputNode.Rows[data.InputNodeRow].InputConnector is not { } inputConnector)
        
        {
            connection = null;
            return false;
        }

        connection = new Connection(outputConnector, inputConnector, nodeTree);
        return true;
    }

    private ConnectionData ComputeData()
    {
        var outputNode = OwningNodeTree.GetParentNode(OutputConnector);
        var outputNodeKey = OwningNodeTree.GetNodeKey(outputNode);
        var outputNodeRow = outputNode.Rows.FindIndex(x => x.OutputConnector == OutputConnector);
        
        var inputNode = OwningNodeTree.GetParentNode(InputConnector);
        var inputNodeKey = OwningNodeTree.GetNodeKey(inputNode);
        var inputNodeRow = inputNode.Rows.FindIndex(x => x.InputConnector == InputConnector);

        if (outputNodeRow == -1 || inputNodeRow == -1)
        {
            throw new InvalidOperationException("Unable to find connectors to create connection data");
        }
        
        return new ConnectionData(outputNodeKey, outputNodeRow, inputNodeKey, inputNodeRow);
    }
}

public record ConnectionData(string OutputNodeId, int OutputNodeRow, string InputNodeId, int InputNodeRow);