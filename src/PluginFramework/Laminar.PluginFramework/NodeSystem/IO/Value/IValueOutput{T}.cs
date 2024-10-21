namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public interface IValueOutput<T> : IValueOutput, IValueProvider<T>
{
    public new T Value { get; set; }

    public bool AlwaysPassUpdate { get; }
}
