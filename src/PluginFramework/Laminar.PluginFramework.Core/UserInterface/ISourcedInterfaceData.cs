using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.UserInterface;

public interface ISourcedInterfaceData<T> : IPersistenceOverrideInterfaceData<T>, IRefreshable, ILaminarExecutionSource where T : notnull
{
    public new bool IsUserEditable { get; set; }
    
    public IValueProvider<T>? ValueProvider { get; set; }

    public IUserInterfaceDefinition? Editor { get; set; }

    public IUserInterfaceDefinition? Viewer { get; set; }

    bool IInterfaceData.IsUserEditable => IsUserEditable;
}