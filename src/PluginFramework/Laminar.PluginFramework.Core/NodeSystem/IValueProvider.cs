namespace Laminar.PluginFramework.NodeSystem;

public interface IValueProvider<out T>
{
    public T Value { get; }
}

public class FuncValueProvider<T>(Func<T> func) : IValueProvider<T>
{
    public T Value => func();
}
