namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public interface IValueInput<T> : IValueInput
{
    public new T Value { get; }
}
