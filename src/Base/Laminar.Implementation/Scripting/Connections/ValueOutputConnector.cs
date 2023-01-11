using System;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueOutputConnector : IOutputConnector<IValueOutput>
{
    readonly ITypeInfoStore _typeInfoStore;
    IValueOutput? _nodeOutput;

    public ValueOutputConnector(ITypeInfoStore typeInfoStore)
{
        _typeInfoStore = typeInfoStore;
    }

    public string ColorHex => _typeInfoStore.GetTypeInfoOrBlank(_nodeOutput!.ValueType).HexColour;

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

    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags)
    {
        if (executionFlags.HasValueFlag())
        {
            return PassUpdateOption.AlwaysPasses;
        }

        return PassUpdateOption.NeverPasses;
    }
}
