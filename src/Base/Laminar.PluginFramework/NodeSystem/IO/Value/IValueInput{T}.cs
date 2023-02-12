namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public interface IValueInput<T> : IValueInput
{
    public new T Value { get; }

    /// <summary>
    /// <para>If <paramref name="valueProvider"/> is <see cref="IValueProvider{T}" of the correct type, returns true and sets value provider/></para>
    /// <para>If <paramref name="valueProvider"/> is null, removes the current value provider and uses the internally stored value and returns true</para>
    /// </summary>
    public void SetValueProvider(IValueProvider<T>? valueProvider);
}
