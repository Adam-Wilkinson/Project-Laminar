namespace Laminar.PluginFramework.UserInterface;

public interface IPersistenceOverrideInterfaceData<T> : IInterfaceData<T> where T : notnull
{
    public T PersistentValue { get; set; }
    
    public PersistenceBehaviour PersistenceBehaviour { get; }
}

public enum PersistenceBehaviour
{
    WhenUserEditable,
    Always,
    Never,
}