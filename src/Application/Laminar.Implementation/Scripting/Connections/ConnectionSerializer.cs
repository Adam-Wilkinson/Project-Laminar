using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Scripting.Connections;

internal class ConnectionSerializer : TypeSerializer<ConnectionData, string>
{
    protected override string SerializeTyped(ConnectionData toSerialize)
    {
        return $"{toSerialize.OutputNodeId}[{toSerialize.OutputNodeRow}] >< {toSerialize.InputNodeId}[{toSerialize.InputNodeRow}]";
    }

    protected override ConnectionData DeSerializeTyped(DeserializationRequest<ConnectionData, string> request)
    {
        var sides = request.Serialized.Split(" >< ");

        var outputParts = sides[0].Split(['[', ']'], StringSplitOptions.RemoveEmptyEntries);
        var inputParts = sides[1].Split(['[', ']'], StringSplitOptions.RemoveEmptyEntries);

        var outputKey = outputParts[0];
        var outputIndex = int.Parse(outputParts[1]);

        var inputKey = inputParts[0];
        var inputIndex = int.Parse(inputParts[1]);

        return new ConnectionData(outputKey, outputIndex, inputKey, inputIndex);
    }
}