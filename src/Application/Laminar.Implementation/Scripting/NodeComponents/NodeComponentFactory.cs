using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeComponents;

internal class NodeComponentFactory : INodeComponentFactory
{
    public INodeComponentCloner<T> Cloner<T>(Func<T> cloner, int startCount) where T : INodeComponent => new NodeRowCloner<T>(cloner, startCount);

    public INodeRow<T> CreateSingleRow<T>(IInput? input, T displayValue, IOutput? output) where T : IInterfaceData =>
        new NodeRow<T>(input, output)
        {
            CentralDisplay = displayValue, 
            InputConnector = input?.Connector, 
            OutputConnector = output?.Connector
        };
}
