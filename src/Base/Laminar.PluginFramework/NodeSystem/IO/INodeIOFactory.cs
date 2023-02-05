using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.PluginFramework.NodeSystem.IO;

public interface INodeIOFactory
{
    public IValueInput<T> ValueInput<T>(string valueName, T initialValue);

    public IValueOutput<T> ValueOutput<T>(string valueName, T initialValue);
}
