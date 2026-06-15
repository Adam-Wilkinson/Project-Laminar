using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginLoader
{
    public void EnsurePluginsLoaded();

    public IRegisteredPlugin GetPluginFor(INode node);
}