using Laminar.PluginFramework.NodeSystem;

namespace Laminar.PluginFramework.UserInterface;

public interface ISourcedInterfaceData<T> : IInterfaceData<T>, IRefreshable, ILaminarExecutionSource where T : notnull
{
    public new bool IsUserEditable { get; set; }

    public IValueProvider<T>? ValueProvider { get; set; }

    bool IInterfaceData.IsUserEditable => IsUserEditable;
}