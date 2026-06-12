namespace Laminar.PluginFramework.UserInterface;

public interface IPersistenceOverrideInterfaceData<T> : IInterfaceData<T> where T : notnull
{
    public T PersistentValue { get; set; }
}