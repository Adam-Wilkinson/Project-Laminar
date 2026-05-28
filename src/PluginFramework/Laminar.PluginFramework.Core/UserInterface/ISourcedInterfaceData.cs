using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.UserInterface;

public interface ISourcedInterfaceData<T> : IInterfaceData<T>, IRefreshable, ILaminarExecutionSource where T : notnull
{
    public new bool IsUserEditable { get; set; }
    
    public IValueProvider<T>? ValueProvider { get; set; }

    public IUserInterfaceDefinition? Editor { get; set; }

    public IUserInterfaceDefinition? Viewer { get; set; }

    /// <summary>
    /// Sets the value even when it is not user editable, and without triggering execution
    /// </summary>
    public void QuietSetValue(T value); 

    bool IInterfaceData.IsUserEditable => IsUserEditable;
}