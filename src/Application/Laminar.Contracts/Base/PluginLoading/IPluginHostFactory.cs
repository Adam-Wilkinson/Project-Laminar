using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginHostFactory
{
    public IReleasablePluginHost GetPluginHost(IInstalledPlugin plugin, ILoadedNodeManager loadedNodeManager);

    public IDisposable CreatePluginRegistrationScope();
}
