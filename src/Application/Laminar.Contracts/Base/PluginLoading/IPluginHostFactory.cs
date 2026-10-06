using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginHostFactory
{
    public IUninstallablePluginHost GetPluginHost(VersionedPluginId pluginId, ILoadedNodeManager loadedNodeManager);

    public IDisposable CreatePluginRegistrationScope();
}
