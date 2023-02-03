using System;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.Connection;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO;

namespace Laminar.Implementation.Scripting.NodeComponents;

internal class NodeComponentFactory : INodeComponentFactory
{
    private readonly IConnectorFactory _connectorViewFactory;
    private readonly IDisplayFactory _displayFactory;

    public NodeComponentFactory(IConnectorFactory connectorViewFactory, IDisplayFactory displayFactory)
    {
        _connectorViewFactory = connectorViewFactory;
        _displayFactory = displayFactory;
    }

    public INodeComponentCloner<T> CreateCloner<T>(Func<T> cloner, int startCount) where T : INodeComponent => new NodeRowCloner<T>(cloner, startCount);

    public INodeRow CreateNodeRow(IInput? input, IValueInfo displayValue, IOutput? output)
    {
        IOutputConnector? outputConnector = output is not null ? _connectorViewFactory.CreateConnector(output) : null;
        IInputConnector? inputConnector = input is not null ? _connectorViewFactory.CreateConnector(input) : null;
        IDisplay display = _displayFactory.CreateDisplayForValue(displayValue);


        return new NodeRow(input, output) { CentralDisplay = display, InputConnector = inputConnector, OutputConnector = outputConnector };
    }
}
