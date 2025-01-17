using System;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeComponents;

internal class NodeComponentFactory(IDisplayFactory displayFactory) : INodeComponentFactory
{
    public INodeComponentCloner<T> Cloner<T>(Func<T> cloner, int startCount) where T : INodeComponent => new NodeRowCloner<T>(cloner, startCount);

    public INodeRow CreateSingleRow(IInput? input, IDisplayValue displayValue, IOutput? output) =>
        new NodeRow(input, output)
        {
            CentralDisplay = displayFactory.CreateDisplayForValue(displayValue), 
            InputConnector = input?.Connector, 
            OutputConnector = output?.Connector
        };
}
