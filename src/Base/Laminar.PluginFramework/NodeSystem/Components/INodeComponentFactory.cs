using System;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Components;

public interface INodeComponentFactory
{
    public INodeComponentCloner<T> Cloner<T>(Func<T> cloner, int startCount) where T : INodeComponent;

    public INodeRow Row(IInput? input, IDisplayValue displayValue, IOutput? output);
}
