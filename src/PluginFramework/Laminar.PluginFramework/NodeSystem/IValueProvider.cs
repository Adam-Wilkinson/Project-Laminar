namespace Laminar.PluginFramework.NodeSystem;

public interface IValueProvider<T>
{
    public T Value { get; }
}
