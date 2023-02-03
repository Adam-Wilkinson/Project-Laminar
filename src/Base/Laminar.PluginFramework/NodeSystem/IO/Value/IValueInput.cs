namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public interface IValueInput : IInput, IValueInfo
{
    /// <summary>
    /// <para>If <paramref name="valueProvider"/> is <see cref="IValueProvider{T}" of the correct type, returns true and sets value provider/></para>
    /// <para>If <paramref name="valueProvider"/> is null, removes the current value provider and uses the internally stored value and returns true</para>
    /// <para>Returns false otherwise</para>
    /// </summary>
    /// <returns>True if the state of the value of the input has changed</returns>
    public bool TrySetValueProvider(object? valueProvider);
}
