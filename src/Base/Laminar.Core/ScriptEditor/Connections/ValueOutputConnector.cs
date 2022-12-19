using System;
using Laminar.Contracts.UserInterface;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.Core.ScriptEditor.Connections;

internal class ValueOutputConnector : IOutputConnector<IValueOutput>
{
    readonly IReadonlyTypeInfoStore _typeInfoStore;
    IValueOutput? _nodeOutput;

    public ValueOutputConnector(IReadonlyTypeInfoStore typeInfoStore)
{
        _typeInfoStore = typeInfoStore;
    }

    public string ColorHex => _typeInfoStore.GetTypeInfo(_nodeOutput!.ValueType).HexColour;

    public IValueOutput Output => _nodeOutput;

    public Action PreEvaluateAction => _nodeOutput!.PreEvaluateAction;

    public void Init(IValueOutput nodeOutput) => _nodeOutput = nodeOutput;

    public void OnDisconnectedFrom(IInputConnector connector)
    {
    }

    public bool TryConnectTo(IInputConnector connector)
    {
        return connector is IInputConnector<IValueInput> valConnector && valConnector.Input.TrySetValueProvider(_nodeOutput);
    }
}
