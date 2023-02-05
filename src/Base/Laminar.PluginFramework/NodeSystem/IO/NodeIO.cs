using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.PluginFramework.NodeSystem.IO;

public static class NodeIO
{
    private static readonly INodeIOFactory Factory = PluginServiceProvider.GetService<INodeIOFactory>();

    public static IValueInput<T> ValueInput<T>(string valueName, T initialValue) => Factory.ValueInput(valueName, initialValue);

    public static IValueOutput<T> ValueOutput<T>(string valueName, T initialValue) => Factory.ValueOutput(valueName, initialValue);
}
