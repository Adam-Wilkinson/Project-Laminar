using System;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Components;

public static class Component
{
    readonly static INodeComponentFactory ComponentFactory = (INodeComponentFactory)PluginServiceProvider.ServiceProvider.GetService(typeof(INodeComponentFactory))!;

    public static INodeRow Row(IInput? input, IDisplayValue displayValue, IOutput? output) => ComponentFactory.CreateNodeRow(input, displayValue, output);

    public static INodeComponentCloner<T> Cloner<T>(Func<T> generator, int startCount) where T : INodeComponent => ComponentFactory.CreateCloner(generator, startCount);
}
