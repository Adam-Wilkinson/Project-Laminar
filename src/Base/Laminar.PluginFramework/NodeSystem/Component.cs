using System;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.PluginFramework.NodeSystem;

public static class Component
{
    readonly static INodeRowFactory rowFactory = (INodeRowFactory)PluginServiceProvider.ServiceProvider.GetService(typeof(INodeRowFactory))!;
    readonly static INodeRowClonerFactory clonerFactory = (INodeRowClonerFactory)PluginServiceProvider.ServiceProvider.GetService(typeof(INodeRowClonerFactory))!;

    public static INodeRow Row(IInput? input, IValueInfo displayValue, IOutput? output) => rowFactory.CreateNodeRow(input, displayValue, output);

    public static INodeRowCloner<T> RowCloner<T>(Func<T> generator, int startCount) where T : IConvertsToNodeRow => clonerFactory.CreateCloner(generator, startCount);
}
