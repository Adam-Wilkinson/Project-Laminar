using System;
using Laminar.PluginFramework.NodeSystem.IO;

namespace Laminar.PluginFramework.NodeSystem.Components;

public interface INodeComponentFactory
{
    public INodeComponentCloner<T> CreateCloner<T>(Func<T> cloner, int startCount) where T : INodeComponent;

    public INodeRow CreateNodeRow(IInput? input, IValueInfo displayValue, IOutput? output);
}
