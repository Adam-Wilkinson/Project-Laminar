using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.PluginFramework.NodeSystem.Contracts.Components;

public interface INodeRowFactory
{
    public INodeRow CreateNodeRow(IInput? input, IValueInfo displayValue, IOutput? output);
}