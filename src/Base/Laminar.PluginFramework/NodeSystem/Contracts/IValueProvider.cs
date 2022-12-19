namespace Laminar.PluginFramework.NodeSystem.Contracts;

public interface IValueProvider<T>
{
    public T Value { get; }
}
