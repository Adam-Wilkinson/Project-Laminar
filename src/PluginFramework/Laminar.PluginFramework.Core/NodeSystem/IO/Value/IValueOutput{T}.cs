using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public interface IValueOutput<T> : IOutput, IValueProvider<T> where T : notnull
{
    public ISourcedInterfaceData<T> InterfaceData { get; }
    
    public new T Value { get; set; }

    public bool AlwaysPassUpdate { get; }
}