using System;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Components;

public interface INodeComponentFactory
{
    public INodeComponentCloner<T> Cloner<T>(Func<T> componentGenerator, int initialComponentCount) where T : INodeComponent;

    public INodeRow CreateSingleRow(IInput? input, IDisplayValue displayValue, IOutput? output);
}
