using System;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.Components;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.PluginFramework.NodeSystem;

public static class Component
{
    readonly static INodeRowFactory rowFactory = (INodeRowFactory)PluginServiceProvider.ServiceProvider.GetService(typeof(INodeRowFactory))!;
    readonly static INodeComponentClonerFactory clonerFactory = (INodeComponentClonerFactory)PluginServiceProvider.ServiceProvider.GetService(typeof(INodeComponentClonerFactory))!;

    public static INodeRow Row(IInput? input, IValueInfo displayValue, IOutput? output) => rowFactory.CreateNodeRow(input, displayValue, output);

    public static INodeComponentCloner<T> Cloner<T>(Func<T> generator, int startCount) where T : INodeComponent => clonerFactory.CreateCloner(generator, startCount);
}
