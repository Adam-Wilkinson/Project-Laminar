using Laminar.PluginFramework.NodeSystem;

namespace Laminar.PluginFramework.UserInterface;

public interface ISourcedInterfaceData<T> : IInterfaceData<T>, IRefreshable, ILaminarExecutionSource where T : notnull
{
    public new bool IsUserEditable { get; set; }
    
    public IValueProvider<T>? ValueProvider { get; set; }

    /// <summary>
    /// Sets the value even when it is not user editable, and does not trigger execution
    /// </summary>
    /// <param name="value"></param>
    public void QuietSetValue(T value); 

    bool IInterfaceData.IsUserEditable => IsUserEditable;
}