using System;
using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.PluginFramework.NodeSystem;

public sealed class NodeComponent
{
    private static INodeRowFactory RowFactory = (INodeRowFactory)PluginServiceProvider.ServiceProvider.GetService(typeof(INodeRowFactory))!;

    private NodeComponent(IInput? input, IValueInfo displayValue, IOutput? output)
    {
        ArgumentNullException.ThrowIfNull(nameof(RowFactory));

        Component = RowFactory.CreateNodeRow(input, displayValue, output);
    }

    private NodeComponent(IEnumerable<NodeComponent> children)
    {
        Component = children;
    }

    public static NodeComponent Row(IInput? input, IValueInfo displayValue, IOutput? output) => new(input, displayValue, output);

    public static NodeComponent Collection(IEnumerable<NodeComponent> children) => new(children);

    public object Component { get; }

    internal INodeRow? SingleRow { get; }

    internal IEnumerable<NodeComponent>? Children { get; }
}
