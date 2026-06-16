using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Scripting.Connections;

internal class ConnectionSerializer : TypeSerializer<Connection, string>
{
    protected override string SerializeTyped(Connection toSerialize)
    {
        var outputNode = toSerialize.OwningNodeTree.GetParentNode(toSerialize.OutputConnector);
        var outputNodeKey = toSerialize.OwningNodeTree.GetNodeKey(outputNode);
        var outputConnectorIndex = outputNode.Rows
            .ToList()
            .FindIndex(x => Equals(x.OutputConnector, toSerialize.OutputConnector));
        
        var inputNode = toSerialize.OwningNodeTree.GetParentNode(toSerialize.InputConnector);
        var inputNodeKey = toSerialize.OwningNodeTree.GetNodeKey(inputNode);
        var inputConnectorIndex = inputNode.Rows
            .ToList()
            .FindIndex(x => Equals(x.InputConnector, toSerialize.InputConnector));

        return $"{outputNodeKey}[{outputConnectorIndex}] >< {inputNodeKey}[{inputConnectorIndex}]";
    }

    protected override Connection DeSerializeTyped(DeserializationRequest<Connection, string> request)
    {
        if (request.Context is not IWritableNodeTree writableNodeTree)
            throw new InvalidOperationException("Deserializing a connection requires a node tree");

        var sides = request.Serialized.Split(" >< ");

        var outputParts = sides[0].Split(['[', ']'], StringSplitOptions.RemoveEmptyEntries);
        var inputParts = sides[1].Split(['[', ']'], StringSplitOptions.RemoveEmptyEntries);

        var outputKey = outputParts[0];
        var outputIndex = int.Parse(outputParts[1]);

        var inputKey = inputParts[0];
        var inputIndex = int.Parse(inputParts[1]);

        var outputNode = writableNodeTree.GetNodeByKey(outputKey);
        var outputConnector = outputNode.Rows[outputIndex].OutputConnector;

        var inputNode = writableNodeTree.GetNodeByKey(inputKey);
        var inputConnector = inputNode.Rows[inputIndex].InputConnector;
        
        if (outputConnector is null || inputConnector is null)
        {
            throw new InvalidOperationException($"The connectors that need connecting are null {outputConnector} and {inputConnector}");
        }

        return new Connection(writableNodeTree)
        {
            OutputConnector = outputConnector,
            InputConnector = inputConnector,
        };
    }
}