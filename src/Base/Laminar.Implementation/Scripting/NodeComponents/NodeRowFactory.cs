using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.Connection;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.Components;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.Implementation.Scripting.NodeComponents;

internal class NodeRowFactory : INodeRowFactory
{
    private readonly IConnectorFactory _connectorViewFactory;
    private readonly IDisplayFactory _displayFactory;

    public NodeRowFactory(IConnectorFactory connectorViewFactory, IDisplayFactory displayFactory)
    {
        _connectorViewFactory = connectorViewFactory;
        _displayFactory = displayFactory;
    }

    public INodeRow CreateNodeRow(IInput? input, IValueInfo displayValue, IOutput? output)
    {
        IOutputConnector? outputConnector = output is not null ? _connectorViewFactory.CreateConnector(output) : null;
        IInputConnector? inputConnector = input is not null ? _connectorViewFactory.CreateConnector(input) : null;
        IDisplay display = _displayFactory.CreateDisplayForValue(displayValue);


        return new NodeRow(input, output) { CentralDisplay = display, InputConnector = inputConnector, OutputConnector = outputConnector };
    }
}
