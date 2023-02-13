using System;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeComponents;

internal class NodeComponentFactory : INodeComponentFactory
{
    private readonly IDisplayFactory _displayFactory;

    public NodeComponentFactory(IDisplayFactory displayFactory)
    {
        _displayFactory = displayFactory;
    }

    public INodeComponentCloner<T> Cloner<T>(Func<T> cloner, int startCount) where T : INodeComponent => new NodeRowCloner<T>(cloner, startCount);

    public INodeRow CreateSingleRow(IInput? input, IDisplayValue displayValue, IOutput? output)
    {
        IDisplay display = _displayFactory.CreateDisplayForValue(displayValue);

        return new NodeRow(input, output) { CentralDisplay = display, InputConnector = input?.Connector, OutputConnector = output?.Connector };
    }
}
